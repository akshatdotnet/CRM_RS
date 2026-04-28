namespace CRM.Core.Interfaces;

public interface IEmailService
{
    Task SendPasswordResetEmailAsync(string toEmail, string toName, string resetLink);
    Task SendWelcomeEmailAsync(string toEmail, string fullName, string tempPassword);
    Task SendLoginAlertEmailAsync(string toEmail, string fullName, string ipAddress, DateTime loginTime);
}
