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

        /// <summary>
        /// Envoie un email récapitulatif des décisions sur les fichiers d'une requête.
        /// </summary>
        public async Task SendRequestReviewNotificationAsync(Request request)
        {
            if (request.RequestedBy == null || string.IsNullOrEmpty(request.RequestedBy.Email))
                return;

            var smtp = _config.GetSection("Smtp");
            var host = smtp["Host"];
            var port = int.TryParse(smtp["Port"], out var p) ? p : 587;
            var user = smtp["User"];
            var password = smtp["Password"];
            var fromEmail = smtp["FromEmail"] ?? "noreply@crealab.fr";
            var fromName = smtp["FromName"] ?? "Créalab";

            if (string.IsNullOrEmpty(host)) return;

            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(request.RequestedBy.FullName, request.RequestedBy.Email));
            message.Subject = $"[Créalab] Retour sur votre demande #{request.Id} — {request.Title}";

            // Construire le corps HTML
            var body = BuildReviewEmailBody(request);
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, MailKit.Security.SecureSocketOptions.StartTls);

            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                await client.AuthenticateAsync(user, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }

        private static string BuildReviewEmailBody(Request request)
        {
            var rows = "";
            foreach (var file in request.Files)
            {
                var statusColor = file.Status switch
                {
                    FileReviewStatus.Accepted => "#2ecc71",
                    FileReviewStatus.Refused => "#e74c3c",
                    FileReviewStatus.NeedsModification => "#f39c12",
                    _ => "#95a5a6"
                };

                var statusIcon = file.Status switch
                {
                    FileReviewStatus.Accepted => "✅",
                    FileReviewStatus.Refused => "❌",
                    FileReviewStatus.NeedsModification => "✏️",
                    _ => "⏳"
                };

                var comment = !string.IsNullOrEmpty(file.ManagerComment)
                    ? $"<br/><small style=\"color:#666;\">{file.ManagerComment}</small>"
                    : "";

                rows += $@"
                <tr>
                    <td style=""padding:8px 12px;border-bottom:1px solid #eee;"">{file.OriginalFileName}</td>
                    <td style=""padding:8px 12px;border-bottom:1px solid #eee;text-align:center;"">
                        <span style=""color:{statusColor};font-weight:bold;"">{statusIcon} {file.StatusLabel}</span>
                        {comment}
                    </td>
                </tr>";
            }

            return $@"
            <div style=""font-family:Arial,sans-serif;max-width:600px;margin:0 auto;padding:20px;"">
                <div style=""background:#0896b5;color:white;padding:20px;border-radius:8px 8px 0 0;text-align:center;"">
                    <h2 style=""margin:0;"">Créalab — Retour sur votre demande</h2>
                </div>
                <div style=""background:white;padding:20px;border:1px solid #e1e8ed;border-top:none;border-radius:0 0 8px 8px;"">
                    <p>Bonjour <strong>{request.RequestedBy?.FirstName}</strong>,</p>
                    <p>L'équipe technique a examiné les fichiers de votre demande <strong>#{request.Id} — {request.Title}</strong>.</p>

                    <table style=""width:100%;border-collapse:collapse;margin:16px 0;"">
                        <thead>
                            <tr style=""background:#f8f9fa;"">
                                <th style=""padding:8px 12px;text-align:left;border-bottom:2px solid #dee2e6;"">Fichier</th>
                                <th style=""padding:8px 12px;text-align:center;border-bottom:2px solid #dee2e6;"">Statut</th>
                            </tr>
                        </thead>
                        <tbody>{rows}</tbody>
                    </table>

                    <p>Connectez-vous sur la plateforme pour voir les détails et, si nécessaire, remplacer les fichiers concernés.</p>
                    <p style=""color:#95a5a6;font-size:0.85rem;"">— L'équipe Créalab</p>
                </div>
            </div>";
        }
    }
}
