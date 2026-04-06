
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yummiez.Models
{
    public class DriverApplication
    {
        public int Id { get; set; }

        public string UserId { get; set; } = null!;

        public string FullName { get; set; } = null!;
        public string LicenseNumber { get; set; } = null!;
        public string VehicleType { get; set; } = null!;
        public string VehicleInfo { get; set; } = null!;

        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}