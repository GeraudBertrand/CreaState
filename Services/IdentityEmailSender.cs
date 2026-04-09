using CreaState.Models;
using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity;
using MimeKit;

namespace CreaState.Services
{
    /// <summary>
    /// Implementation of IEmailSender&lt;User&gt; for ASP.NET Identity email confirmation.
    /// Uses the existing SMTP configuration from appsettings.json.
    /// </summary>
    public class IdentityEmailSender : IEmailSender<User>
    {
        private readonly IConfiguration _config;
        private readonly ILogger<IdentityEmailSender> _logger;

        public IdentityEmailSender(IConfiguration config, ILogger<IdentityEmailSender> logger)
        {
            _config = config;
            _logger = logger;
        }

        public async Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
        {
            var subject = "[Crealab] Confirmez votre adresse email";
            var body = $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
                <div style=""background:#0896b5;color:white;padding:20px;border-radius:8px 8px 0 0;text-align:center;"">
                    <h2 style=""margin:0;"">Crealab ESILV</h2>
                </div>
                <div style=""background:white;padding:20px;border:1px solid #e1e8ed;border-top:none;border-radius:0 0 8px 8px;"">
                    <p>Bonjour <strong>{user.FirstName}</strong>,</p>
                    <p>Merci pour votre inscription sur la plateforme Crealab !</p>
                    <p>Cliquez sur le bouton ci-dessous pour confirmer votre adresse email :</p>
                    <div style=""text-align:center;margin:24px 0;"">
                        <a href=""{confirmationLink}""
                           style=""background:#0896b5;color:white;padding:12px 32px;border-radius:8px;text-decoration:none;font-weight:bold;display:inline-block;"">
                            Confirmer mon email
                        </a>
                    </div>
                    <p style=""color:#95a5a6;font-size:0.85rem;"">Si vous n'avez pas demande cette inscription, ignorez ce message.</p>
                    <p style=""color:#95a5a6;font-size:0.85rem;"">— L'equipe Crealab</p>
                </div>
            </div>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
        {
            var subject = "[Crealab] Reinitialisation de votre mot de passe";
            var body = $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
                <div style=""background:#0896b5;color:white;padding:20px;border-radius:8px 8px 0 0;text-align:center;"">
                    <h2 style=""margin:0;"">Crealab ESILV</h2>
                </div>
                <div style=""background:white;padding:20px;border:1px solid #e1e8ed;border-top:none;border-radius:0 0 8px 8px;"">
                    <p>Bonjour <strong>{user.FirstName}</strong>,</p>
                    <p>Vous avez demande la reinitialisation de votre mot de passe.</p>
                    <div style=""text-align:center;margin:24px 0;"">
                        <a href=""{resetLink}""
                           style=""background:#e67e22;color:white;padding:12px 32px;border-radius:8px;text-decoration:none;font-weight:bold;display:inline-block;"">
                            Reinitialiser le mot de passe
                        </a>
                    </div>
                    <p style=""color:#95a5a6;font-size:0.85rem;"">Si vous n'avez pas fait cette demande, ignorez ce message.</p>
                </div>
            </div>";

            await SendEmailAsync(email, subject, body);
        }

        public async Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
        {
            var subject = "[Crealab] Code de reinitialisation";
            var body = $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
                <p>Bonjour <strong>{user.FirstName}</strong>,</p>
                <p>Votre code de reinitialisation : <strong>{resetCode}</strong></p>
            </div>";

            await SendEmailAsync(email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string htmlBody)
        {
            var smtp = _config.GetSection("Smtp");
            var host = smtp["Host"];
            var port = int.TryParse(smtp["Port"], out var p) ? p : 587;
            var user = smtp["User"];
            var password = smtp["Password"];
            var fromEmail = smtp["FromEmail"] ?? "noreply@crealab.fr";
            var fromName = smtp["FromName"] ?? "Crealab";

            if (string.IsNullOrEmpty(host))
            {
                _logger.LogWarning("SMTP host not configured. Email to {Email} not sent.", toEmail);
                return;
            }

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = htmlBody };

            try
            {
                using var client = new SmtpClient();
                await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);

                if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                    await client.AuthenticateAsync(user, password);

                await client.SendAsync(message);
                await client.DisconnectAsync(true);

                _logger.LogInformation("Email sent to {Email}: {Subject}", toEmail, subject);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {Email}", toEmail);
                throw;
            }
        }
    }
}
