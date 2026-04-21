using System.ComponentModel.DataAnnotations;

namespace Yummiez.Models
{
    public class CartItem
    {
        [Required]
        public int RestaurantId { get; set; }

        [Required]
        [StringLength(100)]
        public string RestaurantName { get; set; } = string.Empty;

        [Required]
        [StringLength(120)]
        public string ItemName { get; set; } = string.Empty;

        [Range(0.01, 1000)]
        public decimal UnitPrice { get; set; }

        [Range(1, 50)]
        public int Quantity { get; set; } = 1;

        public decimal TotalPrice => UnitPrice * Quantity;
    }
}
