namespace SmtpEmailService.Application.Features.Emails.SendEmail;

public sealed class SendEmailHandler(IEmailService emailService)
{
    public Task<SendEmailResult> HandleAsync(SendEmailRequest request, CancellationToken cancellationToken) =>
        emailService.SendAsync(request, cancellationToken);
}
