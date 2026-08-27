using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using SmtpEmailService.Api;
using SmtpEmailService.Application;
using SmtpEmailService.Application.Features.Emails.SendEmail;
using SmtpEmailService.Infrastructure;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "SMTP Email Service API",
        Version = "v1",
        Description = "Sends emails through the configured SMTP server."
    });
    options.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Name = "X-Api-Key",
        Type = SecuritySchemeType.ApiKey,
        In = ParameterLocation.Header,
        Description = "Enter the API key used to call protected endpoints."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        [new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "ApiKey" } }] = []
    });
});
builder.Services.AddHealthChecks();
builder.Services.AddProblemDetails();

var app = builder.Build();
app.UseExceptionHandler(exceptionApp => exceptionApp.Run(async context =>
{
    var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
    var (status, title, detail, errors) = exception switch
    {
        RequestValidationException validation => (StatusCodes.Status400BadRequest, "Validation failed", "One or more fields are invalid.", validation.Errors),
        EmailDeliveryException => (StatusCodes.Status502BadGateway, "Email delivery failed", "The SMTP server could not accept the email.", null),
        _ => (StatusCodes.Status500InternalServerError, "Internal server error", "An unexpected error occurred.", null)
    };
    context.Response.StatusCode = status;
    await context.Response.WriteAsJsonAsync(errors is null
        ? new ProblemDetails { Status = status, Title = title, Detail = detail }
        : new HttpValidationProblemDetails(errors) { Status = status, Title = title, Detail = detail });
}));

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "SMTP Email Service API v1");
        options.DocumentTitle = "SMTP Email Service API";
    });
}
app.UseHttpsRedirection();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();
app.MapHealthChecks("/health").AllowAnonymous();
app.Run();

public partial class Program;
