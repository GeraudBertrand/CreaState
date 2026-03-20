using System.ComponentModel.DataAnnotations;

namespace CreaState.Models
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// Code machine unique (ex: "manage_inventory", "admin_access").
        /// </summary>
        [Required, MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string DisplayName { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Catégorie pour le regroupement UI (Navigation, Demandes, Inventaire, etc.)
        /// </summary>
        [MaxLength(50)]
        public string Category { get; set; } = string.Empty;

        public ICollection<RolePermission> RolePermissions { get; set; } = [];
    }
}
