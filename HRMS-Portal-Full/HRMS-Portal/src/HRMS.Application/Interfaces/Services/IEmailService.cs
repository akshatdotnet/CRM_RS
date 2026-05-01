namespace HRMS.Application.Interfaces.Services;

public interface IEmailService
{
    Task<bool> SendAsync(EmailMessage message, CancellationToken ct = default);
    Task<bool> SendWithAttachmentAsync(EmailMessage message, byte[] attachment, string attachmentName, CancellationToken ct = default);
}

public record EmailMessage(
    string To,
    string ToName,
    string Subject,
    string Body,
    bool IsHtml = true,
    string? Cc = null
);
