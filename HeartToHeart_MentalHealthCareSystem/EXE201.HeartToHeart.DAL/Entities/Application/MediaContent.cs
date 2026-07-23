using EXE201.HeartToHeart.DAL.Entities.Base;
using System.ComponentModel.DataAnnotations;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class MediaContent : AuditableEntityBase
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        [MaxLength(50)]
        public string? Type { get; set; } // Video, Audio, Article, etc.

        [MaxLength(500)]
        public string? Url { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public bool IsPremium { get; set; } = false;

        public int ViewCount { get; set; } = 0;

        // New properties for enhanced functionality
        [MaxLength(500)]
        public string? ThumbnailUrl { get; set; }

        public int? DurationSeconds { get; set; } // For video/audio content

        [MaxLength(100)]
        public string? Source { get; set; } // YouTube, Spotify, Internal, etc.

        public string? ExternalId { get; set; } // External platform ID

        public DateTime? PublishedAt { get; set; }

        // Navigation Properties
        public virtual ICollection<MediaContentTag> MediaContentTags { get; set; } = new List<MediaContentTag>();
    }
}
