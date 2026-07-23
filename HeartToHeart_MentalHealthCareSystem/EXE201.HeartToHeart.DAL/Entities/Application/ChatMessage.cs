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
    public class ChatMessage : EntityBase
    {
        public Guid SessionId { get; set; }
        public Guid SenderId { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsFromAI { get; set; } = false;
        public bool IsRead { get; set; } = false;

        public virtual ChatSession Session { get; set; } = null!;
        public virtual ApplicationUser Sender { get; set; } = null!;
    }
}
