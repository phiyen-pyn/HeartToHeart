using EXE201.HeartToHeart.DAL.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Identity
{
    public class RefreshToken : EntityBase
    {
        public Guid UserId { get; set; }

        [Required]
        [MaxLength(500)]
        public string Token { get; set; } = string.Empty;

        [Required]
        [MaxLength(500)]
        public string JwtId { get; set; } = string.Empty;

        public bool IsUsed { get; set; } = false;
        public bool IsRevoked { get; set; } = false;
        public DateTime ExpiryDate { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
    }
}
