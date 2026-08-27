using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SmtpEmailService.Application.Features.Emails.SendEmail;

namespace SmtpEmailService.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<SmtpOptions>().Bind(configuration.GetSection(SmtpOptions.SectionName)).ValidateOnStart();
        services.AddScoped<IEmailSender, MailKitEmailSender>();
        return services;
    }
}
