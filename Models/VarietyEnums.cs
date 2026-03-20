using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CreaState.Models
{
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
        [Display(Name = "Termniné")]
        Success,
        [Display(Name = "Erreur")]
        Error
    }


    public enum PrintStatus
    {
        [Display(Name = "Terminé")]
        Success,
        [Display(Name = "Échec")]
        Failed,
        [Display(Name = "Annulé")]
        Cancelled
    }

    /// <summary>
    /// Énumération des différentes années au sein de l'école
    /// </summary>
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

    public enum InventoryCategory
    {
        [Display(Name = "Filament")]
        Filament,
        [Display(Name = "Résine")]
        Resin,
        [Display(Name = "Matériau laser")]
        LaserMaterial,
        [Display(Name = "Électronique")]
        Electronics,
        [Display(Name = "Autre")]
        Other
    }

    public enum MaintenanceType
    {
        [Display(Name = "Planifiée")]
        Scheduled,
        [Display(Name = "Réparation")]
        Repair,
        [Display(Name = "Calibration")]
        Calibration,
        [Display(Name = "Nettoyage")]
        Cleaning,
        [Display(Name = "Autre")]
        Other
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

    /// <summary>
    /// Méthode statique pour obtenir le nom par l'attribut d'une valeur d'un Enumérateur.
    /// </summary>
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
