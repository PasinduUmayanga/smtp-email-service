using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpEmailService.Application.Features.Emails.SendEmail;
using SmtpEmailService.Domain;

namespace SmtpEmailService.Infrastructure;

public sealed class MailKitEmailSender(IOptions<SmtpOptions> options, ILogger<MailKitEmailSender> logger) : IEmailSender
{
    public async Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        ValidateSettings(settings);
        ValidateAttachments(message, settings);
        var mimeMessage = CreateMimeMessage(message, settings);

        try
        {
            using var client = new SmtpClient();
            client.Timeout = checked(settings.TimeoutSeconds * 1000);
            var socketOptions = settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto;
            await client.ConnectAsync(settings.Host, settings.Port, socketOptions, cancellationToken);
            if (!string.IsNullOrWhiteSpace(settings.Username))
                await client.AuthenticateAsync(settings.Username, settings.Password, cancellationToken);
            await client.SendAsync(mimeMessage, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);
            logger.LogInformation("Email {MessageId} submitted to SMTP server for {RecipientCount} recipients.", mimeMessage.MessageId, message.To.Count + message.Cc.Count + message.Bcc.Count);
            return mimeMessage.MessageId ?? throw new EmailDeliveryException("The SMTP server accepted an email without a message identifier.");
        }
        catch (Exception exception) when (exception is SmtpCommandException or SmtpProtocolException or IOException or TimeoutException)
        {
            logger.LogWarning(exception, "SMTP delivery failed.");
            throw new EmailDeliveryException("The SMTP server could not accept the email.", exception);
        }
    }

    private static MimeMessage CreateMimeMessage(EmailMessage message, SmtpOptions settings)
    {
        var mime = new MimeMessage();
        mime.From.Add(new MailboxAddress(settings.FromName, settings.FromAddress));
        AddRecipients(mime.To, message.To); AddRecipients(mime.Cc, message.Cc); AddRecipients(mime.Bcc, message.Bcc);
        if (message.ReplyTo is not null) mime.ReplyTo.Add(new MailboxAddress(message.ReplyTo.DisplayName, message.ReplyTo.Address));
        mime.Subject = message.Subject;
        var body = new BodyBuilder { TextBody = message.PlainTextBody, HtmlBody = message.HtmlBody };
        foreach (var attachment in message.Attachments) body.Attachments.Add(attachment.FileName, attachment.Content, ContentType.Parse(attachment.ContentType));
        mime.Body = body.ToMessageBody();
        return mime;
    }

    private static void AddRecipients(InternetAddressList destination, IEnumerable<EmailAddress> addresses)
    {
        foreach (var address in addresses) destination.Add(new MailboxAddress(address.DisplayName, address.Address));
    }

    private static void ValidateSettings(SmtpOptions settings)
    {
        if (string.IsNullOrWhiteSpace(settings.Host) || string.IsNullOrWhiteSpace(settings.FromAddress))
            throw new InvalidOperationException("SMTP host and from address must be configured.");
    }

    private static void ValidateAttachments(EmailMessage message, SmtpOptions settings)
    {
        if (message.Attachments.Count > settings.MaxAttachmentCount || message.Attachments.Sum(x => (long)x.Content.Length) > settings.MaxTotalAttachmentBytes)
            throw new RequestValidationException(new Dictionary<string, string[]> { ["attachments"] = ["Attachment limits were exceeded."] });
    }
}
