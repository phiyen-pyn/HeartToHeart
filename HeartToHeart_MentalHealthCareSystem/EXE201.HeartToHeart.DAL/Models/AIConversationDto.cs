using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class AIConversationDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string UserMessage { get; set; } = string.Empty;
        public string? AIResponse { get; set; }
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
    }
}
