using EXE201.HeartToHeart.DAL.Entities.Base;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Entities.Application
{
    public class Holiday : EntityBase
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public DateTime Date { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsRecurring { get; set; } = false; // For holidays that repeat yearly

        public bool IsActive { get; set; } = true;

        [MaxLength(50)]
        public string? CountryCode { get; set; } = "VN"; // Default to Vietnam
    }
}
