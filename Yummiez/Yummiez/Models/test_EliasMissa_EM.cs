using System;
using System.ComponentModel.DataAnnotations;

namespace Yummiez.Models
{
    public class TestEliasMissaEM
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string ProjectName { get; set; } = null!;

        public string? Description { get; set; }

        public decimal Budget { get; set; }

        [Display(Name = "Start Date")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [Display(Name = "Is Active")]
        public bool IsActive { get; set; }
    }
}
