using Microsoft.Extensions.DependencyInjection;
using SmtpEmailService.Application.Features.Emails.SendEmail;

namespace SmtpEmailService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IEmailService, EmailService>();
        services.AddScoped<SendEmailHandler>();
        return services;
    }
}
