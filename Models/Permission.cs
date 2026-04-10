using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        // Navigation
        public ICollection<RolePermission> RolePermissions { get; set; } = [];
    }
}
