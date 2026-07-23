using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class BlogSearchDto
    {
        public string? Title { get; set; }
        public string? Category { get; set; }
        public string? Tag { get; set; }
        public Guid? UserId { get; set; }
        public bool? IsPublished { get; set; }
        public bool? IsFeatured { get; set; }
        public bool? IsPremium { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; } = "CreatedAt"; // CreatedAt, ViewCount, LikeCount, PublishedAt
        public string? SortDirection { get; set; } = "desc"; // asc, desc
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
