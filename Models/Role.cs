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

        /// <summary>
        /// Si true, ce rôle est assigné par défaut aux nouveaux inscrits (Élève).
        /// </summary>
        public bool IsDefault { get; set; } = false;

        public ICollection<RolePermission> RolePermissions { get; set; } = [];
    }
}
