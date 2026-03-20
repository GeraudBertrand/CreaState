using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CreaState.Models
{
    public class InventoryItem
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public InventoryCategory Category { get; set; } = InventoryCategory.Filament;

        [MaxLength(100)]
        public string? Brand { get; set; }

        [MaxLength(50)]
        public string? Color { get; set; }

        [MaxLength(50)]
        public string? MaterialType { get; set; }

        public double QuantityRemaining { get; set; }

        public double InitialQuantity { get; set; }

        [MaxLength(10)]
        public string Unit { get; set; } = "g";

        public double LowStockThreshold { get; set; }

        public DateTime LastUpdated { get; set; } = DateTime.UtcNow;

        public string? Notes { get; set; }

        [NotMapped]
        public bool IsLowStock => QuantityRemaining <= LowStockThreshold;

        [NotMapped]
        public double PercentageRemaining => InitialQuantity > 0
            ? Math.Round(QuantityRemaining / InitialQuantity * 100, 1)
            : 0;

        [NotMapped]
        public string StockCssClass => IsLowStock ? "text-danger" : PercentageRemaining < 50 ? "text-warning" : "text-success";
    }
}
