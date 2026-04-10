using Microsoft.AspNetCore.Identity;

namespace CreaState.Models
{
    /// <summary>
    /// Table de jointure Identity User ↔ Role, avec navigations.
    /// Remplace l'ancien MembreRole.
    /// </summary>
    public class AppUserRole : IdentityUserRole<int>
    {
        public User User { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }
}
