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
    public class AIConversation : EntityBase
    {
        public Guid UserId { get; set; }

        [Required]
        public string UserMessage { get; set; } = string.Empty;

        public string? AIResponse { get; set; }

        [MaxLength(50)]
        public string? EmotionDetected { get; set; }

        public DateTime SentAt { get; set; } = DateTime.UtcNow;

        public float? SentimentScore { get; set; } // -1 to 1 (negative to positive)

        public virtual ApplicationUser User { get; set; } = null!;
    }
}
