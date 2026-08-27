# SMTP Email Service

An ASP.NET Core .NET 10 SMTP email API using Clean/Layered Architecture. `POST /api/emails` sends an email synchronously through the configured SMTP server.

## Run

Set secrets outside source control, then run the API:

```powershell
dotnet user-secrets --project src/SmtpEmailService.Api set "Smtp:Password" "your-smtp-password"
dotnet user-secrets --project src/SmtpEmailService.Api set "ApiKey:Value" "a-long-random-api-key"
dotnet run --project src/SmtpEmailService.Api
```

Set `Smtp:Host`, `Smtp:Username`, and `Smtp:FromAddress` through environment variables or local configuration. In Development, Swagger UI is available at `/swagger`; use **Authorize** to enter the `X-Api-Key` value. Health is available at `/health`.

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
