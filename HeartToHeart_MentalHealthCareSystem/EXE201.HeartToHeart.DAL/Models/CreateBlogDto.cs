using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class CreateBlogDto
    {
        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Content { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Summary { get; set; }

        [StringLength(500)]
        public string? FeaturedImage { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public List<string> Tags { get; set; } = new List<string>();

        public bool IsPublished { get; set; } = false;

        public bool IsFeatured { get; set; } = false;

        public bool IsPremium { get; set; } = false;
    }
}
