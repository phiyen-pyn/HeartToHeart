using EXE201.HeartToHeart.DAL.Entities.Base;
using EXE201.HeartToHeart.DAL.Entities.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class GoogleCalendarToken : EntityBase
    {
        [Required]
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(1000)]
        public string AccessToken { get; set; } = string.Empty;

        [MaxLength(1000)]
        public string? RefreshToken { get; set; }

        [Required]
        public DateTime ExpiresAt { get; set; }

        public bool IsRevoked { get; set; } = false;

        [MaxLength(500)]
        public string? Scope { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        // Helper property
        public bool IsExpired => DateTime.UtcNow >= ExpiresAt.AddMinutes(-5); // 5-minute buffer
    }
}
