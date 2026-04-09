using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CreaState.Models
{
    public enum UserType
    {
        [Display(Name = "Élève")]
        Eleve,
        [Display(Name = "Membre")]
        Membre
    }

    public enum ClassYearEnum
    {
        [Display(Name = "A1")]
        A1,
        [Display(Name = "A2")]
        A2,
        [Display(Name = "A3")]
        A3,
        [Display(Name = "A4")]
        A4,
        [Display(Name = "A5")]
        A5,
        [Display(Name = "Alumni")]
        Alumni,
        [Display(Name = "Autre")]
        Other
    }

    public enum RequestType
    {
        [Display(Name = "Impression FDM")]
        FDM,
        [Display(Name = "Impression SLA")]
        SLA,
        [Display(Name = "Découpe laser")]
        Laser,
        [Display(Name = "Autre")]
        Other
    }

    public enum RequestStatus
    {
        [Display(Name = "Soumise")]
        Submitted,
        [Display(Name = "En cours d'examen")]
        UnderReview,
        [Display(Name = "Approuvée")]
        Approved,
        [Display(Name = "Rejetée")]
        Rejected,
        [Display(Name = "En cours")]
        InProgress,
        [Display(Name = "Terminée")]
        Completed,
        [Display(Name = "Annulée")]
        Cancelled
    }

    public enum MaintenanceType
    {
        [Display(Name = "Réparation")]
        Reparation,
        [Display(Name = "Calibration")]
        Calibration,
        [Display(Name = "Nettoyage")]
        Nettoyage,
        [Display(Name = "Autre")]
        Autre
    }

    public enum FileReviewStatus
    {
        [Display(Name = "En attente")]
        Pending,
        [Display(Name = "Accepté")]
        Accepted,
        [Display(Name = "Refusé")]
        Refused,
        [Display(Name = "À modifier")]
        NeedsModification
    }

    public enum PrinterStatus
    {
        [Display(Name = "Hors ligne")]
        Offline,
        [Display(Name = "Prête")]
        Idle,
        [Display(Name = "Imprime")]
        Printing,
        [Display(Name = "Pause")]
        Pause,
        [Display(Name = "Terminé")]
        Success,
        [Display(Name = "Erreur")]
        Error
    }

    public static class EnumHelper
    {
        public static string GetDisplayName(this Enum enumValue)
        {
            return enumValue.GetType()
                            .GetMember(enumValue.ToString())
                            .First()
                            .GetCustomAttribute<DisplayAttribute>()?
                            .Name ?? enumValue.ToString();
        }
    }
}
