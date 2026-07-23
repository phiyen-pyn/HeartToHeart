using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class UpdateBlogDto
    {
        [StringLength(200)]
        public string? Title { get; set; }

        public string? Content { get; set; }

        [StringLength(500)]
        public string? Summary { get; set; }

        [StringLength(500)]
        public string? FeaturedImage { get; set; }

        [StringLength(100)]
        public string? Category { get; set; }

        public List<string>? Tags { get; set; }

        public bool? IsPublished { get; set; }

        public bool? IsFeatured { get; set; }

        public bool? IsPremium { get; set; }
    }
}
