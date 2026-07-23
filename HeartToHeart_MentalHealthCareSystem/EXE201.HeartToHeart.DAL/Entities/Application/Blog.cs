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
    public class Blog : AuditableEntityBase
    {
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Summary { get; set; }

        [MaxLength(500)]
        public string? FeaturedImage { get; set; }

        [MaxLength(100)]
        public string? Category { get; set; }

        public string? Tags { get; set; } // JSON array of tags

        public bool IsPublished { get; set; } = false;

        public DateTime? PublishedAt { get; set; }

        public int ViewCount { get; set; } = 0;

        public int LikeCount { get; set; } = 0;

        public bool IsFeatured { get; set; } = false;

        public bool IsPremium { get; set; } = false;

        // User as author (staff member)
        public Guid UserId { get; set; }

        // Navigation Properties
        public virtual ApplicationUser User { get; set; } = null!;
        public virtual ICollection<BlogComment> Comments { get; set; } = new List<BlogComment>();
        public virtual ICollection<BlogLike> Likes { get; set; } = new List<BlogLike>();
        public virtual ICollection<BlogTag> BlogTags { get; set; } = new List<BlogTag>();
    }
}
