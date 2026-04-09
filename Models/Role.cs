using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace CreaState.Models
{
    public class Role : IdentityRole<int>
    {
        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        // Navigation
        public ICollection<RolePermission> RolePermissions { get; set; } = [];
        public ICollection<AppUserRole> UserRoles { get; set; } = [];
    }
}
