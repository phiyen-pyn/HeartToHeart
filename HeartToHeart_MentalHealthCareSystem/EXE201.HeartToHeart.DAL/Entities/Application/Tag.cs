using EXE201.HeartToHeart.DAL.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class Tag : EntityBase
    {
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? Description { get; set; }

        [MaxLength(20)]
        public string? Color { get; set; } // Hex color for UI

        public int UsageCount { get; set; } = 0;

        // Navigation Properties
        public virtual ICollection<BlogTag> BlogTags { get; set; } = new List<BlogTag>();
        public virtual ICollection<MediaContentTag> MediaContentTags { get; set; } = new List<MediaContentTag>();
    }
}
