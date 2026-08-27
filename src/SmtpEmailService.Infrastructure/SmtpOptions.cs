namespace SmtpEmailService.Infrastructure;

public sealed class SmtpOptions
{
    public const string SectionName = "Smtp";
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public bool UseStartTls { get; init; } = true;
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string? FromName { get; init; }
    public int TimeoutSeconds { get; init; } = 30;
    public int MaxAttachmentCount { get; init; } = 10;
    public long MaxTotalAttachmentBytes { get; init; } = 10 * 1024 * 1024;
}
