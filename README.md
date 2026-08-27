# SMTP Email Service

[![Build status](https://ci.appveyor.com/api/projects/status/1220154/branch/main?svg=true)](https://ci.appveyor.com/project/Mahadenamuththa/smtp-email-service/branch/main)
[![Build History](https://img.shields.io/badge/AppVeyor-Build%20History-blue?logo=appveyor)](https://ci.appveyor.com/project/Mahadenamuththa/smtp-email-service/history)

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet&logoColor=white)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4?logo=dotnet&logoColor=white)
![MailKit](https://img.shields.io/badge/MailKit-4.17.0-0078D4?logo=maildotru&logoColor=white)
![Swagger](https://img.shields.io/badge/Swagger-6.9.0-85EA2D?logo=swagger&logoColor=black)
![xUnit](https://img.shields.io/badge/xUnit-2.9.2-5A3E85?logo=xunit&logoColor=white)

An ASP.NET Core .NET 10 SMTP email API using Clean/Layered Architecture. `POST /api/emails` sends an email synchronously through the configured SMTP server.

## Prerequisites and versions

- **Target framework:** .NET 10 (`net10.0`), the current LTS release.
- **Required SDK:** .NET SDK 10.0 or later within the 10.x release line.
- **ASP.NET Core runtime:** 10.0 or later within the 10.x release line.
- Dependency versions are shown in the badges above; MailKit includes MimeKit 4.17.0.

## Run

Set secrets outside source control, then run the API:

```powershell
dotnet user-secrets --project src/SmtpEmailService.Api set "Smtp:Password" "your-smtp-password"
dotnet user-secrets --project src/SmtpEmailService.Api set "ApiKey:Value" "a-long-random-api-key"
dotnet run --project src/SmtpEmailService.Api
```

Set `Smtp:Host`, `Smtp:Username`, and `Smtp:FromAddress` through environment variables or local configuration. In Development, Swagger UI is available at `/swagger`; use **Authorize** to enter the `X-Api-Key` value. Health is available at `/health`.

Run the automated tests with:

```powershell
dotnet test SmtpEmailService.sln
```

## Gmail SMTP setup

1. Enable [Google 2-Step Verification](https://myaccount.google.com/signinoptions/two-step-verification).
2. Create a 16-digit [Google App Password](https://myaccount.google.com/apppasswords) for this service. Do not use your normal Google password.
3. Configure the service using .NET user secrets:

```powershell
dotnet user-secrets --project src/SmtpEmailService.Api set "Smtp:Host" "smtp.gmail.com"
dotnet user-secrets --project src/SmtpEmailService.Api set "Smtp:Port" "587"
dotnet user-secrets --project src/SmtpEmailService.Api set "Smtp:UseStartTls" "true"
dotnet user-secrets --project src/SmtpEmailService.Api set "Smtp:Username" "your-address@gmail.com"
dotnet user-secrets --project src/SmtpEmailService.Api set "Smtp:Password" "your-16-digit-app-password"
dotnet user-secrets --project src/SmtpEmailService.Api set "Smtp:FromAddress" "your-address@gmail.com"
dotnet user-secrets --project src/SmtpEmailService.Api set "Smtp:FromName" "SMTP Email Service"
dotnet user-secrets --project src/SmtpEmailService.Api set "ApiKey:Value" "a-long-random-api-key"
```

The configured `FromAddress` must be the Gmail account address or a Gmail-verified send-as alias. For a Google Workspace account, App Password availability can be restricted by the organization administrator or Advanced Protection.

## Send an email

```http
POST /api/emails
X-Api-Key: a-long-random-api-key
Content-Type: application/json

{
  "to": ["recipient@example.com"],
  "subject": "Hello",
  "plainTextBody": "Hello from the SMTP email service.",
  "htmlBody": "<p>Hello from the SMTP email service.</p>",
  "attachments": [
    { "fileName": "note.txt", "contentType": "text/plain", "contentBase64": "SGVsbG8=" }
  ]
}
```

The configured sender address is always used. Optional `cc`, `bcc`, and `replyTo` fields are supported. The endpoint returns `202 Accepted` after the SMTP server accepts the message; delivery errors return `502`.
