using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class BlogDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? FeaturedImage { get; set; }
        public string? Category { get; set; }
        public List<string> Tags { get; set; } = new List<string>();
        public bool IsPublished { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int ViewCount { get; set; }
        public int LikeCount { get; set; }
        public bool IsFeatured { get; set; }
        public bool IsPremium { get; set; }
        public Guid UserId { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
        public bool IsActive { get; set; }
        public List<BlogCommentDto> Comments { get; set; } = new List<BlogCommentDto>();
        public bool IsLikedByCurrentUser { get; set; }
    }
}
