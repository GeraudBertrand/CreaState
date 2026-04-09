using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsDefault { get; set; } = false;

        // Navigation
        public ICollection<RolePermission> RolePermissions { get; set; } = [];
        public ICollection<MembreRole> MembreRoles { get; set; } = [];
    }
}
