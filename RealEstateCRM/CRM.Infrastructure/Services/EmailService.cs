using CRM.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Net.Mail;

namespace CRM.Infrastructure.Services;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
    {
        var subject = "Reset Your RealEstate CRM Password";
        var body = $@"
<!DOCTYPE html><html><body style='font-family:Segoe UI,sans-serif;background:#f8f9fa;padding:20px;'>
<div style='max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:32px;border:1px solid #e9ecef;'>
  <div style='font-size:22px;font-weight:600;color:#185FA5;margin-bottom:8px;'>RealEstate CRM</div>
  <h2 style='font-size:18px;font-weight:500;margin-bottom:16px;'>Password Reset Request</h2>
  <p style='color:#555;line-height:1.6;'>Hi {toName},</p>
  <p style='color:#555;line-height:1.6;'>We received a request to reset your password. Click the button below to set a new password. This link expires in <strong>1 hour</strong>.</p>
  <div style='text-align:center;margin:28px 0;'>
    <a href='{resetLink}' style='background:#185FA5;color:#fff;padding:12px 28px;border-radius:8px;text-decoration:none;font-weight:500;font-size:14px;'>Reset Password</a>
  </div>
  <p style='color:#888;font-size:12px;line-height:1.6;'>If you didn't request this, you can safely ignore this email. Your password won't change.</p>
  <p style='color:#888;font-size:12px;'>Or copy this link: <a href='{resetLink}' style='color:#185FA5;word-break:break-all;'>{resetLink}</a></p>
  <hr style='border:none;border-top:1px solid #e9ecef;margin:24px 0;'>
  <p style='color:#aaa;font-size:11px;text-align:center;'>RealEstate CRM · Enterprise System</p>
</div></body></html>";
        await SendAsync(toEmail, toName, subject, body);
    }

    public async Task SendWelcomeEmailAsync(string toEmail, string fullName, string tempPassword)
    {
        var subject = "Welcome to RealEstate CRM — Your Account is Ready";
        var body = $@"
<!DOCTYPE html><html><body style='font-family:Segoe UI,sans-serif;background:#f8f9fa;padding:20px;'>
<div style='max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:32px;border:1px solid #e9ecef;'>
  <div style='font-size:22px;font-weight:600;color:#185FA5;margin-bottom:8px;'>RealEstate CRM</div>
  <h2 style='font-size:18px;font-weight:500;'>Welcome, {fullName}!</h2>
  <p style='color:#555;line-height:1.6;'>Your account has been created. Here are your login credentials:</p>
  <div style='background:#f8f9fa;border-radius:8px;padding:16px;margin:20px 0;font-family:monospace;'>
    <div><strong>Email:</strong> {toEmail}</div>
    <div><strong>Temporary Password:</strong> {tempPassword}</div>
  </div>
  <p style='color:#e74c3c;font-size:13px;'>Please change your password immediately after first login.</p>
  <hr style='border:none;border-top:1px solid #e9ecef;margin:24px 0;'>
  <p style='color:#aaa;font-size:11px;text-align:center;'>RealEstate CRM · Enterprise System</p>
</div></body></html>";
        await SendAsync(toEmail, fullName, subject, body);
    }

    public async Task SendLoginAlertEmailAsync(string toEmail, string fullName, string ipAddress, DateTime loginTime)
    {
        var subject = "New Login Detected — RealEstate CRM";
        var body = $@"
<!DOCTYPE html><html><body style='font-family:Segoe UI,sans-serif;background:#f8f9fa;padding:20px;'>
<div style='max-width:520px;margin:auto;background:#fff;border-radius:12px;padding:32px;border:1px solid #e9ecef;'>
  <div style='font-size:22px;font-weight:600;color:#185FA5;margin-bottom:8px;'>RealEstate CRM</div>
  <h2 style='font-size:18px;font-weight:500;'>New Login Alert</h2>
  <p style='color:#555;line-height:1.6;'>Hi {fullName}, a new login was detected on your account.</p>
  <div style='background:#f8f9fa;border-radius:8px;padding:16px;margin:20px 0;font-size:13px;'>
    <div><strong>Time:</strong> {loginTime:MMM dd, yyyy hh:mm tt} UTC</div>
    <div><strong>IP Address:</strong> {ipAddress}</div>
  </div>
  <p style='color:#555;font-size:13px;'>If this wasn't you, please <a href='/Account/ForgotPassword' style='color:#185FA5;'>reset your password</a> immediately.</p>
  <hr style='border:none;border-top:1px solid #e9ecef;margin:24px 0;'>
  <p style='color:#aaa;font-size:11px;text-align:center;'>RealEstate CRM · Enterprise System</p>
</div></body></html>";
        await SendAsync(toEmail, fullName, subject, body);
    }

    private async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        try
        {
            var smtpHost = _config["Email:SmtpHost"] ?? "smtp.gmail.com";
            var smtpPort = int.Parse(_config["Email:SmtpPort"] ?? "587");
            var smtpUser = _config["Email:SmtpUser"] ?? "";
            var smtpPass = _config["Email:SmtpPass"] ?? "";
            var fromEmail = _config["Email:FromEmail"] ?? smtpUser;
            var fromName = _config["Email:FromName"] ?? "RealEstate CRM";

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };
            using var msg = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };
            msg.To.Add(new MailAddress(toEmail, toName));
            await client.SendMailAsync(msg);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
            // Don't re-throw — email failure should not crash the request
        }
    }
}

/// <summary>No-op email service for development (logs instead of sending).</summary>
public class LogEmailService : IEmailService
{
    private readonly ILogger<LogEmailService> _logger;
    public LogEmailService(ILogger<LogEmailService> logger) => _logger = logger;

    public Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink)
    { _logger.LogInformation("[EMAIL] Password reset for {Email}: {Link}", toEmail, resetLink); return Task.CompletedTask; }

    public Task SendWelcomeEmailAsync(string toEmail, string fullName, string tempPassword)
    { _logger.LogInformation("[EMAIL] Welcome for {Email} | Temp pw: {Pw}", toEmail, tempPassword); return Task.CompletedTask; }

    public Task SendLoginAlertEmailAsync(string toEmail, string fullName, string ipAddress, DateTime loginTime)
    { _logger.LogInformation("[EMAIL] Login alert for {Email} from {IP}", toEmail, ipAddress); return Task.CompletedTask; }
}
