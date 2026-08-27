using SmtpEmailService.Application.Features.Emails.SendEmail;
using SmtpEmailService.Domain;
using Xunit;

namespace SmtpEmailService.Tests;

public sealed class SendEmailHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidRequest_DelegatesToSender()
    {
        var sender = new RecordingSender();
        var handler = new SendEmailHandler(new EmailService(sender));
        var result = await handler.HandleAsync(new SendEmailRequest(["recipient@example.com"], null, null, "Subject", "Text", null, null), CancellationToken.None);

        Assert.Equal("message-id", result.MessageId);
        Assert.Equal("recipient@example.com", sender.Message!.To.Single().Address);
    }

    [Fact]
    public async Task HandleAsync_NoRecipients_ThrowsValidationException()
    {
        var handler = new SendEmailHandler(new EmailService(new RecordingSender()));
        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => handler.HandleAsync(new SendEmailRequest([], null, null, "Subject", "Text", null, null), CancellationToken.None));

        Assert.Contains("recipients", exception.Errors.Keys);
    }

    [Fact]
    public async Task HandleAsync_InvalidAttachment_ThrowsValidationException()
    {
        var handler = new SendEmailHandler(new EmailService(new RecordingSender()));
        var request = new SendEmailRequest(["recipient@example.com"], null, null, "Subject", "Text", null, [new AttachmentRequest("file.txt", "text/plain", "not base64")]);

        var exception = await Assert.ThrowsAsync<RequestValidationException>(() => handler.HandleAsync(request, CancellationToken.None));
        Assert.Contains("attachments", exception.Errors.Keys);
    }

    private sealed class RecordingSender : IEmailSender
    {
        public EmailMessage? Message { get; private set; }
        public Task<string> SendAsync(EmailMessage message, CancellationToken cancellationToken) { Message = message; return Task.FromResult("message-id"); }
    }
}
