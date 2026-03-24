using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yummiez.Models
{
    [Table("Drivers")]
    public class Driver
    {
        [Key]
        [Column("driver_id")]
        public int DriverId { get; set; }

        [Required]
        [Column("identity_user_id")]
        [MaxLength(450)]
        public string IdentityUserId { get; set; } = null!;

        [Column("license_number")]
        [MaxLength(50)]
        public string? LicenseNumber { get; set; }

        [Column("vehicle_type")]
        [MaxLength(50)]
        public string? VehicleType { get; set; }

        [Column("is_available")]
        public bool IsAvailable { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
