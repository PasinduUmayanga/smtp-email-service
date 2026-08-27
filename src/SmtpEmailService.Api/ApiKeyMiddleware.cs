using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

namespace SmtpEmailService.Api;

public sealed class ApiKeyOptions
{
    public const string SectionName = "ApiKey";
    public string Value { get; init; } = string.Empty;
}

public sealed class ApiKeyMiddleware(RequestDelegate next, IConfiguration configuration)
{
    private const string HeaderName = "X-Api-Key";

    public async Task InvokeAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api")) { await next(context); return; }
        var expected = configuration.GetValue<string>($"{ApiKeyOptions.SectionName}:Value");
        if (string.IsNullOrWhiteSpace(expected) || !context.Request.Headers.TryGetValue(HeaderName, out var supplied) || !CryptographicOperations.FixedTimeEquals(System.Text.Encoding.UTF8.GetBytes(expected), System.Text.Encoding.UTF8.GetBytes(supplied.ToString())))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new ProblemDetails { Status = StatusCodes.Status401Unauthorized, Title = "Unauthorized", Detail = "A valid X-Api-Key header is required." });
            return;
        }
        await next(context);
    }
}
