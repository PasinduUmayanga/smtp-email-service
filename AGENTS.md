# SMTP Email Service Agent Guide

## Project commands

Use the SDK pinned in `global.json`.

```powershell
dotnet restore SmtpEmailService.sln
dotnet build SmtpEmailService.sln --configuration Release --no-restore
dotnet test SmtpEmailService.sln --configuration Release --no-build --no-restore
```

Run the API with `dotnet run --project src/SmtpEmailService.Api`. Swagger UI is available at `/swagger` in the Development environment.

## Architecture and dependency rules

- `Domain` contains framework-independent email models only.
- `Application` contains use cases under `Features/<Feature>/<UseCase>`. Controllers call handlers; handlers delegate to services; services depend only on application abstractions.
- `Infrastructure` implements external concerns. SMTP delivery stays in `MailKitEmailSender`; do not reference MailKit/MimeKit from Application or API.
- `Api` contains HTTP controllers, middleware, request handling, and dependency composition. Keep controllers thin: no validation, SMTP, or business orchestration there.
- Dependencies flow inward: `Api -> Application <- Infrastructure`; Domain is the innermost layer.

## Email and security conventions

- Preserve the configured SMTP sender policy; callers may use `ReplyTo` but must not control `From`.
- Do not log email bodies, attachment contents, SMTP passwords, or API keys.
- Keep credentials and `ApiKey:Value` out of source control. Use environment variables or .NET user secrets.
- Return `RequestValidationException` for safe client validation failures and `EmailDeliveryException` for SMTP transport failures so the API maps them to the correct problem response.

## Change and test expectations

- Add or update tests in `tests/SmtpEmailService.Tests` for application behavior changes.
- Keep package versions compatible with .NET 10 and run the full build and test commands before handoff.
- Do not commit generated `bin`, `obj`, `.vs`, test results, local settings, or secret files.
