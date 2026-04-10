using CreaState.Models;
using MailKit.Net.Smtp;
using MimeKit;

namespace CreaState.Services
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendRequestReviewNotificationAsync(Requete requete)
        {
            if (requete.Demandeur == null || string.IsNullOrEmpty(requete.Demandeur.Email))
                return;

            var smtp = _config.GetSection("Smtp");
            var host = smtp["Host"];
            var port = int.TryParse(smtp["Port"], out var p) ? p : 587;
            var user = smtp["User"];
            var password = smtp["Password"];
            var fromEmail = smtp["FromEmail"] ?? "noreply@crealab.fr";
            var fromName = smtp["FromName"] ?? "Créalab";

            if (string.IsNullOrEmpty(host)) return;

            var demandeur = requete.Demandeur;
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress($"{demandeur.FirstName} {demandeur.LastName}", demandeur.Email));
            message.Subject = $"[Créalab] Retour sur votre demande #{requete.Id} — {requete.Title}";

            var body = BuildReviewEmailBody(requete);
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);

            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                await client.AuthenticateAsync(user, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private static string BuildReviewEmailBody(Requete requete)
        {
            var rows = "";
            foreach (var file in requete.Fichiers)
            {
                var statusColor = file.ReviewStatus switch
                {
                    FileReviewStatus.Accepted => "#2ecc71",
                    FileReviewStatus.Refused => "#e74c3c",
                    FileReviewStatus.NeedsModification => "#f39c12",
                    _ => "#95a5a6"
                };

                var statusIcon = file.ReviewStatus switch
                {
                    FileReviewStatus.Accepted => "OK",
                    FileReviewStatus.Refused => "X",
                    FileReviewStatus.NeedsModification => "~",
                    _ => "?"
                };

                rows += $@"
                <tr>
                    <td style=""padding:8px 12px;border-bottom:1px solid #eee;"">{file.FileName}</td>
                    <td style=""padding:8px 12px;border-bottom:1px solid #eee;text-align:center;"">
                        <span style=""color:{statusColor};font-weight:bold;"">{statusIcon} {file.ReviewStatus.GetDisplayName()}</span>
                    </td>
                </tr>";
            }

            return $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
                <div style=""background:#0896b5;color:white;padding:20px;border-radius:8px 8px 0 0;text-align:center;"">
                    <h2 style=""margin:0;"">Créalab — Retour sur votre demande</h2>
                </div>
                <div style=""background:white;padding:20px;border:1px solid #e1e8ed;border-top:none;border-radius:0 0 8px 8px;"">
                    <p>Bonjour <strong>{requete.Demandeur?.FirstName}</strong>,</p>
                    <p>L'équipe technique a examiné les fichiers de votre demande <strong>#{requete.Id} — {requete.Title}</strong>.</p>
                    <table style=""width:100%;border-collapse:collapse;margin:16px 0;"">
                        <thead>
                            <tr style=""background:#f8f9fa;"">
                                <th style=""padding:8px 12px;text-align:left;border-bottom:2px solid #dee2e6;"">Fichier</th>
                                <th style=""padding:8px 12px;text-align:center;border-bottom:2px solid #dee2e6;"">Statut</th>
                            </tr>
                        </thead>
                        <tbody>{rows}</tbody>
                    </table>
                    <p>Connectez-vous sur la plateforme pour voir les détails.</p>
                    <p style=""color:#95a5a6;font-size:0.85rem;"">— L'équipe Créalab</p>
                </div>
            </div>";
        }
    }
}
