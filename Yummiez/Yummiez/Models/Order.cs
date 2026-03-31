using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yummiez.Models
{
    [Table("Orders")]
    public class Order
    {
        [Key]
        [Column("order_id")]
        public int OrderId { get; set; }

        [Required]
        [Column("user_id")]
        [MaxLength(450)]
        public string UserId { get; set; } = null!;

        [Column("restaurant_id")]
        public int RestaurantId { get; set; }

        [ForeignKey("RestaurantId")]
        public Restaurant? Restaurant { get; set; }

        [Required]
        [Column("delivery_address")]
        [MaxLength(250)]
        public string DeliveryAddress { get; set; } = null!;

        [Column("status")]
        [MaxLength(20)]
        public string Status { get; set; } = "Placed";

        // Simulated driver coordinates
        [Column("driver_lat")]
        public double DriverLat { get; set; }

        [Column("driver_lng")]
        public double DriverLng { get; set; }

        // Destination coordinates
        [Column("dest_lat")]
        public double DestLat { get; set; }

        [Column("dest_lng")]
        public double DestLng { get; set; }

        // Restaurant (start) coordinates
        [Column("restaurant_lat")]
        public double RestaurantLat { get; set; }

        [Column("restaurant_lng")]
        public double RestaurantLng { get; set; }

        [Column("driver_name")]
        [MaxLength(100)]
        public string DriverName { get; set; } = "Alex M.";

        [Column("step_count")]
        public int StepCount { get; set; } = 0;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Column("delivered_at")]
        public DateTime? DeliveredAt { get; set; }
    }

    public static class OrderStatus
    {
        public const string Placed = "Placed";
        public const string Preparing = "Preparing";
        public const string PickedUp = "PickedUp";
        public const string OnTheWay = "OnTheWay";
        public const string Delivered = "Delivered";
    }
}
