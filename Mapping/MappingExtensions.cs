using System.Globalization;
using CreaState.DTOs.Consommables;
using CreaState.DTOs.Evenements;
using CreaState.DTOs.Formations;
using CreaState.DTOs.Maintenance;
using CreaState.DTOs.Printers;
using CreaState.DTOs.Requetes;
using CreaState.DTOs.Roles;
using CreaState.DTOs.Users;
using CreaState.Models;

namespace CreaState.Mapping
{
    public static class MappingExtensions
    {
        // === User ===
        public static UserDto ToDto(this User user) => new()
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            ClassYear = user.ClassYear.GetDisplayName(),
            UserType = user.UserType.GetDisplayName(),
            FullName = $"{user.FirstName} {user.LastName}",
            Initials = string.IsNullOrEmpty(user.FirstName) || string.IsNullOrEmpty(user.LastName)
                ? "??"
                : $"{user.FirstName[0]}{user.LastName[0]}".ToUpper()
        };

        // === Membre ===
        public static MembreDto ToDto(this Membre membre) => new()
        {
            Id = membre.Id,
            FirstName = membre.FirstName,
            LastName = membre.LastName,
            Email = membre.Email,
            ClassYear = membre.ClassYear.GetDisplayName(),
            UserType = membre.UserType.GetDisplayName(),
            FullName = $"{membre.FirstName} {membre.LastName}",
            Initials = string.IsNullOrEmpty(membre.FirstName) || string.IsNullOrEmpty(membre.LastName)
                ? "??"
                : $"{membre.FirstName[0]}{membre.LastName[0]}".ToUpper(),
            JoinDate = membre.JoinDate,
            IsActive = membre.IsActive,
            Roles = membre.MembreRoles.Select(mr => mr.Role!.ToDto()).ToList(),
            RoleLabel = string.Join(", ", membre.MembreRoles.Select(mr => mr.Role?.DisplayName ?? "")),
            IsBoardMember = membre.MembreRoles.Any(mr => mr.Role?.Name != "Eleve"),
            AvatarColor = membre.MembreRoles.Any(mr => mr.Role?.Name != "Eleve")
                ? "var(--accent-magenta)"
                : "var(--primary-blue)"
        };

        // === Role ===
        public static RoleDto ToDto(this Role role) => new()
        {
            Id = role.Id,
            Name = role.Name,
            DisplayName = role.DisplayName,
            Description = role.Description,
            Permissions = role.RolePermissions.Select(rp => rp.Permission.ToDto()).ToList()
        };

        // === Permission ===
        public static PermissionDto ToDto(this Permission permission) => new()
        {
            Id = permission.Id,
            Code = permission.Code,
            DisplayName = permission.DisplayName,
            Category = permission.Category
        };

        // === Consommable ===
        public static ConsommableDto ToDto(this Consommable c) => new()
        {
            Id = c.Id,
            Type = c.Type,
            Quantite = c.Quantite,
            Seuil = c.Seuil,
            CouleurNom = c.CouleurNom,
            CouleurHex = c.CouleurHex,
            IsLowStock = c.Quantite <= c.Seuil
        };

        // === Printer ===
        public static PrinterDto ToDto(this Printer p) => new()
        {
            Id = p.Id,
            Name = p.Name,
            IpAddress = p.IpAddress,
            Model = p.Model,
            SerialNumber = p.SerialNumber,
            Enabled = p.Enabled
        };

        // === Formation ===
        public static FormationDto ToDto(this Formation f)
        {
            var endTime = f.StartTime.Add(f.Duration);
            return new FormationDto
            {
                Id = f.Id,
                Titre = f.Titre,
                Description = f.Description,
                InstructeurId = f.InstructeurId,
                Instructeur = f.Instructeur != null ? f.Instructeur.ToDto() : null,
                Date = f.Date,
                StartTime = f.StartTime,
                Duration = f.Duration,
                MaxParticipants = f.MaxParticipants,
                EndTime = endTime,
                FormattedTimeRange = $"{f.StartTime:HH\\hmm} - {endTime:HH\\hmm}",
                DayBadge = f.Date.Day.ToString("00"),
                MonthBadge = f.Date.ToString("MMM", CultureInfo.CreateSpecificCulture("fr-FR")).ToUpper()
            };
        }

        // === Evenement ===
        public static EvenementDto ToDto(this Evenement e)
        {
            var daysRemaining = (e.Date.Date - DateTime.Now.Date).Days;
            return new EvenementDto
            {
                Id = e.Id,
                Title = e.Title,
                Description = e.Description,
                Date = e.Date,
                Location = e.Location,
                Icone = e.Icone,
                DaysRemaining = daysRemaining,
                IsUpcoming = daysRemaining >= 0,
                CountdownLabel = daysRemaining switch
                {
                    0 => "Aujourd'hui !",
                    1 => "Demain",
                    _ => $"J-{daysRemaining}"
                },
                BadgeCssClass = daysRemaining <= 3 ? "badge-urgent" : "badge-normal"
            };
        }

        // === Maintenance ===
        public static MaintenanceDto ToDto(this Models.Maintenance m) => new()
        {
            Id = m.Id,
            PrinterId = m.PrinterId,
            Printer = m.Printer?.ToDto(),
            WorkerId = m.WorkerId,
            Worker = m.Worker != null ? m.Worker.ToDto() : null,
            Type = m.Type.ToString(),
            TypeLabel = m.Type.GetDisplayName(),
            Description = m.Description,
            Date = m.Date
        };

        // === Requete ===
        public static RequeteDto ToDto(this Requete r) => new()
        {
            Id = r.Id,
            Type = r.Type.ToString(),
            TypeLabel = r.Type.GetDisplayName(),
            Status = r.Status.ToString(),
            StatusLabel = r.Status.GetDisplayName(),
            StatusCssClass = r.Status switch
            {
                RequestStatus.Submitted => "badge-info",
                RequestStatus.UnderReview => "badge-warning",
                RequestStatus.Approved => "badge-success",
                RequestStatus.Rejected => "badge-danger",
                RequestStatus.InProgress => "badge-primary",
                RequestStatus.Completed => "badge-success",
                RequestStatus.Cancelled => "badge-secondary",
                _ => ""
            },
            Title = r.Title,
            Description = r.Description,
            RejectionReason = r.RejectionReason,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            DemandeurId = r.DemandeurId,
            Demandeur = r.Demandeur?.ToDto(),
            AssigneId = r.AssigneId,
            Assigne = r.Assigne != null ? r.Assigne.ToDto() : null,
            Fichiers = r.Fichiers.Select(f => f.ToDto()).ToList(),
            Commentaires = r.Commentaires.Select(c => c.ToDto()).ToList()
        };

        // === RequeteFichier ===
        public static RequeteFichierDto ToDto(this RequeteFichier f) => new()
        {
            Id = f.Id,
            RequeteId = f.RequeteId,
            FileName = f.FileName,
            FilePath = f.FilePath,
            FileSize = f.FileSize,
            ReviewStatus = f.ReviewStatus.ToString(),
            ReviewStatusLabel = f.ReviewStatus.GetDisplayName(),
            StatusCssClass = f.ReviewStatus switch
            {
                FileReviewStatus.Pending => "file-pending",
                FileReviewStatus.Accepted => "file-accepted",
                FileReviewStatus.Refused => "file-refused",
                FileReviewStatus.NeedsModification => "file-modify",
                _ => ""
            },
            FileSizeLabel = f.FileSize switch
            {
                < 1024 => $"{f.FileSize} o",
                < 1024 * 1024 => $"{f.FileSize / 1024.0:F1} Ko",
                _ => $"{f.FileSize / 1024.0 / 1024.0:F2} Mo"
            },
            UploadedAt = f.UploadedAt
        };

        // === RequeteCommentaire ===
        public static RequeteCommentaireDto ToDto(this RequeteCommentaire c) => new()
        {
            Id = c.Id,
            RequeteId = c.RequeteId,
            AuteurId = c.AuteurId,
            Auteur = c.Auteur?.ToDto(),
            Contenu = c.Contenu,
            Date = c.Date
        };

        // === PrintJob ===
        public static PrintJobDto ToDto(this PrintJob pj) => new()
        {
            Id = pj.Id,
            FileName = pj.FileName,
            PrinterId = pj.PrinterId,
            PrinterName = pj.Printer?.Name ?? "",
            RequeteId = pj.RequeteId,
            StartTime = pj.StartTime,
            Duration = pj.Duration,
            DurationLabel = $"{(int)pj.Duration.TotalHours}h {pj.Duration.Minutes:00}m",
            EndTime = pj.StartTime.Add(pj.Duration),
            Status = pj.Status.ToString(),
            StatusLabel = pj.Status.GetDisplayName(),
            StatusCssClass = pj.Status switch
            {
                PrintStatus.Success => "status-success",
                PrintStatus.Failed => "status-failed",
                PrintStatus.Cancelled => "status-cancelled",
                _ => ""
            },
            FilamentWeightGrams = pj.FilamentWeightGrams
        };
    }
}
