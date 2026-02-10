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

    /// <summary>
    /// Énumération des différents rôles de l'association
    /// </summary>
    public enum RoleEnum
    {
        [Display(Name = "Membre")]
        Member,

        [Display(Name = "Président")]
        President,

        [Display(Name = "Vice-Président")]
        VicePresident,

        [Display(Name = "Trésorier")]
        Treasurer,

        [Display(Name = "Secrétaire")]
        Secretary,

        [Display(Name = "Resp. Partenariat")]
        PartnershipManager,

        [Display(Name = "Resp. Technique")]
        TechManager,

        [Display(Name = "Resp. Communication")]
        ComManager,

        [Display(Name = "Resp. Événementiel")]
        EventManager
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
