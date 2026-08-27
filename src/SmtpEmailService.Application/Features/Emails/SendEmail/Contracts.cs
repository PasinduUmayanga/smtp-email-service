using System.Net.Mail;
using SmtpEmailService.Domain;

namespace SmtpEmailService.Application.Features.Emails.SendEmail;

public sealed record AttachmentRequest(string FileName, string ContentType, string ContentBase64);

public sealed record SendEmailRequest(
    IReadOnlyCollection<string>? To,
    IReadOnlyCollection<string>? Cc,
    IReadOnlyCollection<string>? Bcc,
    string? Subject,
    string? PlainTextBody,
    string? HtmlBody,
    IReadOnlyCollection<AttachmentRequest>? Attachments,
    string? ReplyTo = null);

public sealed record SendEmailResult(string MessageId);

public interface IEmailService
{
    Task<SendEmailResult> SendAsync(SendEmailRequest request, CancellationToken cancellationToken);
}

public interface IEmailSender
{
    Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken);
}

public sealed class RequestValidationException(IReadOnlyDictionary<string, string[]> errors) : Exception("The email request is invalid.")
{
    public IReadOnlyDictionary<string, string[]> Errors { get; } = errors;
}

public sealed class EmailDeliveryException(string message, Exception? innerException = null) : Exception(message, innerException);

internal static class EmailRequestMapper
{
    public static EmailMessage MapAndValidate(SendEmailRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);
        var errors = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        var to = ParseAddresses(request.To, "to", errors);
        var cc = ParseAddresses(request.Cc, "cc", errors);
        var bcc = ParseAddresses(request.Bcc, "bcc", errors);
        var replyTo = ParseSingleAddress(request.ReplyTo, "replyTo", errors);

        if (to.Count + cc.Count + bcc.Count == 0) errors["recipients"] = ["At least one recipient is required."];
        if (string.IsNullOrWhiteSpace(request.Subject)) errors["subject"] = ["Subject is required."];
        if (string.IsNullOrWhiteSpace(request.PlainTextBody) && string.IsNullOrWhiteSpace(request.HtmlBody)) errors["body"] = ["Plain-text or HTML body is required."];

        var attachments = ParseAttachments(request.Attachments, errors);
        if (errors.Count > 0) throw new RequestValidationException(errors);
        return new EmailMessage(to, cc, bcc, request.Subject!.Trim(), request.PlainTextBody, request.HtmlBody, attachments, replyTo);
    }

    private static List<EmailAddress> ParseAddresses(IReadOnlyCollection<string>? values, string field, Dictionary<string, string[]> errors)
    {
        var addresses = new List<EmailAddress>();
        foreach (var value in values ?? [])
        {
            if (TryParseAddress(value, out var address)) addresses.Add(address!);
            else errors[field] = ["One or more email addresses are invalid."];
        }
        return addresses;
    }

    private static EmailAddress? ParseSingleAddress(string? value, string field, Dictionary<string, string[]> errors)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        if (TryParseAddress(value, out var address)) return address;
        errors[field] = ["Email address is invalid."];
        return null;
    }

    private static bool TryParseAddress(string? value, out EmailAddress? address)
    {
        address = null;
        if (string.IsNullOrWhiteSpace(value)) return false;
        try
        {
            var parsed = new MailAddress(value);
            if (!string.Equals(parsed.Address, value.Trim(), StringComparison.OrdinalIgnoreCase)) return false;
            address = new EmailAddress(parsed.Address, parsed.DisplayName);
            return true;
        }
        catch (FormatException) { return false; }
    }

    private static List<EmailAttachment> ParseAttachments(IReadOnlyCollection<AttachmentRequest>? values, Dictionary<string, string[]> errors)
    {
        var attachments = new List<EmailAttachment>();
        foreach (var value in values ?? [])
        {
            if (string.IsNullOrWhiteSpace(value.FileName) || string.IsNullOrWhiteSpace(value.ContentType))
            {
                errors["attachments"] = ["Attachments require a file name and content type."];
                continue;
            }
            try { attachments.Add(new EmailAttachment(value.FileName, value.ContentType, Convert.FromBase64String(value.ContentBase64))); }
            catch (FormatException) { errors["attachments"] = ["One or more attachments contain invalid base64 content."]; }
        }
        return attachments;
    }
}
