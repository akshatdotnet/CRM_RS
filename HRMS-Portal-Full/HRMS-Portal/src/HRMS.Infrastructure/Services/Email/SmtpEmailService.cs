using System.Net;
using System.Net.Mail;
using HRMS.Application.Interfaces.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HRMS.Infrastructure.Services.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config; _logger = logger;
    }

    public async Task<bool> SendAsync(EmailMessage message, CancellationToken ct = default)
        => await SendWithAttachmentAsync(message, Array.Empty<byte>(), string.Empty, ct);

    public async Task<bool> SendWithAttachmentAsync(EmailMessage message, byte[] attachment,
        string attachmentName, CancellationToken ct = default)
    {
        var isMock = bool.Parse(_config["Email:Mock"] ?? "true");
        if (isMock)
        {
            _logger.LogInformation("[MOCK EMAIL] To: {To} | Subject: {Subject} | Attachment: {Att}",
                message.To, message.Subject, attachmentName);
            return true;
        }
        try
        {
            using var client = new SmtpClient(_config["Email:Host"], int.Parse(_config["Email:Port"] ?? "587"))
            {
                Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
                EnableSsl = true, Timeout = 10000
            };
            using var mail = new MailMessage
            {
                From = new MailAddress(_config["Email:From"]!, _config["Email:FromName"] ?? "HR Team"),
                Subject = message.Subject, Body = message.Body, IsBodyHtml = message.IsHtml
            };
            mail.To.Add(new MailAddress(message.To, message.ToName));
            if (!string.IsNullOrEmpty(message.Cc)) mail.CC.Add(message.Cc);
            if (attachment.Length > 0)
                mail.Attachments.Add(new Attachment(new MemoryStream(attachment), attachmentName, "application/pdf"));
            await client.SendMailAsync(mail, ct);
            _logger.LogInformation("Email sent to {To}", message.To);
            return true;
        }
        catch (Exception ex) { _logger.LogError(ex, "Email failed to {To}", message.To); return false; }
    }
}
