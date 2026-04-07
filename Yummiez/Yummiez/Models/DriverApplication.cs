
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;


namespace Yummiez.Models
{
    public class DriverApplication
    {
        public int Id { get; set; }


        [ValidateNever]
        public string UserId { get; set; } = null!;

        [Required]
        public string FullName { get; set; } = null!;

        [Required]
        public string LicenseNumber { get; set; } = null!;

        [Required]
        public string VehicleType { get; set; } = null!;

        [Required]
        public string VehicleInfo { get; set; } = null!;

        [Required]
        public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}