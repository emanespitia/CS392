using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yummiez.Models
{
    [Table("Restaurants")]
    public class Restaurant
    {
        [Key]
        [Column("restaurant_id")]
        public int RestaurantId { get; set; }

        [Required]
        [Column("name")]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        [Column("owner_name")]
        [MaxLength(100)]
        public string OwnerName { get; set; } = null!;

        [Required]
        [Column("address")]
        [MaxLength(500)]
        public string Address { get; set; } = null!;

        [Column("phone")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("is_open")]
        public bool? IsOpen { get; set; }

        [Column("admin_id")]
        public int AdminId { get; set; }

        [Column("manager_user_id")]
        [MaxLength(450)]
        public string? ManagerUserId { get; set; }

        [Column("category")]
        [MaxLength(50)]
        public string? Category { get; set; }

        [Column("image_url")]
        [MaxLength(500)]
        public string? ImageUrl { get; set; }

        [Column("menu_items_json")]
        public string? MenuItemsJson { get; set; }

        [Column("created_at")]
        public DateTime? CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }
}