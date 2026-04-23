using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace Yummiez.Models
{
    public class DriverApplication
    {
        public int Id { get; set; }

        [ValidateNever]
        public string UserId { get; set; } = null!;

        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "License number is required")]
        [StringLength(50, ErrorMessage = "License number is too long")]
        public string LicenseNumber { get; set; } = null!;

        [Required(ErrorMessage = "Vehicle type is required")]
        [RegularExpression("^(Car|Bike|Scooter)$", ErrorMessage = "Please select a valid vehicle type.")]
        public string VehicleType { get; set; } = null!;

        [Required(ErrorMessage = "Vehicle info is required")]
        [StringLength(200, ErrorMessage = "Vehicle info must be under 200 characters")]
        public string VehicleInfo { get; set; } = null!;

        [Required]
        public string Status { get; set; } = "Pending";

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
