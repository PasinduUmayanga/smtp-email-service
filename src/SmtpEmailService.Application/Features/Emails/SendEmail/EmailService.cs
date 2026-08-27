namespace SmtpEmailService.Application.Features.Emails.SendEmail;

public sealed class EmailService(IEmailSender emailSender) : IEmailService
{
    public async Task<SendEmailResult> SendAsync(SendEmailRequest request, CancellationToken cancellationToken)
    {
        var message = EmailRequestMapper.MapAndValidate(request);
        return new SendEmailResult(await emailSender.SendAsync(message, cancellationToken));
    }
}
