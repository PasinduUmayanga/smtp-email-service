namespace SmtpEmailService.Domain;

public sealed record EmailAddress(string Address, string? DisplayName = null);

public sealed record EmailAttachment(string FileName, string ContentType, byte[] Content);

public sealed record EmailMessage(
    IReadOnlyCollection<EmailAddress> To,
    IReadOnlyCollection<EmailAddress> Cc,
    IReadOnlyCollection<EmailAddress> Bcc,
    string Subject,
    string? PlainTextBody,
    string? HtmlBody,
    IReadOnlyCollection<EmailAttachment> Attachments,
    EmailAddress? ReplyTo = null);
