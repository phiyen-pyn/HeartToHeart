using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class BlogCommentDto
    {
        public Guid Id { get; set; }
        public Guid BlogId { get; set; }
        public Guid? UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public Guid? ParentCommentId { get; set; }
        public bool IsApproved { get; set; }
        public string? UserName { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<BlogCommentDto> Replies { get; set; } = new List<BlogCommentDto>();
    }
}
