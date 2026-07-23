using EXE201.HeartToHeart.DAL.Entities.Application;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Identity
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        public ApplicationUser()
        {
            Id = Guid.NewGuid();
            SecurityStamp = Guid.NewGuid().ToString();
        }

        [MaxLength(100)]
        [Required]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [MaxLength(20)]
        public string? Gender { get; set; }

        [MaxLength(500)]
        public string? ProfilePicture { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public virtual ICollection<AnonymousPost> AnonymousPosts { get; set; } = new List<AnonymousPost>();
        public virtual ICollection<DiaryEntry> DiaryEntries { get; set; } = new List<DiaryEntry>();
        public virtual ICollection<EmotionTrack> EmotionTracks { get; set; } = new List<EmotionTrack>();
        public virtual ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public virtual ICollection<AIConversation> AIConversations { get; set; } = new List<AIConversation>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<PaymentTransaction> PaymentTransactions { get; set; } = new List<PaymentTransaction>();
        public virtual ICollection<Report> Reports { get; set; } = new List<Report>();
        public virtual ICollection<Appointment> UserAppointments { get; set; } = new List<Appointment>();
        public virtual ICollection<Notification> Notifications { get; set; } = new List<Notification>();

        // Counselor specific navigation
        public virtual Counselor? CounselorProfile { get; set; }

        // New Blog-related Navigation Properties
        public virtual ICollection<Blog> Blogs { get; set; } = new List<Blog>();
        public virtual ICollection<BlogComment> BlogComments { get; set; } = new List<BlogComment>();
        public virtual ICollection<BlogLike> BlogLikes { get; set; } = new List<BlogLike>();
        public virtual ICollection<AppointmentHistory> AppointmentHistoryChanges { get; set; } = new List<AppointmentHistory>();
    }
}
