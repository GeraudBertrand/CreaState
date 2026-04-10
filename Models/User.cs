using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CreaState.Models
{
    public class User : IdentityUser<int>
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        public ClassYearEnum ClassYear { get; set; } = ClassYearEnum.Other;

        public UserType UserType { get; set; } = UserType.Eleve;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? LastLoginAt { get; set; }

        // Navigation vers les rôles (via la table de jointure Identity)
        public ICollection<AppUserRole> UserRoles { get; set; } = [];
    }
}
