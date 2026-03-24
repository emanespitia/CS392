using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Yummiez.Models
{
    [Table("Clients")]
    public class Client
    {
        [Key]
        [Column("client_id")]
        public int ClientId { get; set; }

        [Required]
        [Column("identity_user_id")]
        [MaxLength(450)]
        public string IdentityUserId { get; set; } = null!;

        [Column("display_name")]
        [MaxLength(150)]
        public string? DisplayName { get; set; }

        [Column("phone")]
        [MaxLength(20)]
        public string? Phone { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; } = true;

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
