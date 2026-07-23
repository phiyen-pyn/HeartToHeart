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
    public class ChatSession : AuditableEntityBase
    {
        public Guid UserId { get; set; }
        public Guid CounselorId { get; set; }

        public DateTime StartedAt { get; set; } = DateTime.UtcNow;
        public DateTime? EndedAt { get; set; }

        [MaxLength(50)]
        public string Status { get; set; } = "Active"; // Active, Ended, Paused

        public string? SessionNotes { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;
        public virtual Counselor Counselor { get; set; } = null!;
        public virtual ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }
}
