using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class EmotionTrackDto
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Emotion { get; set; } = string.Empty;
        public string? Note { get; set; }
        public int? IntensityLevel { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; }
        public string? CreatedBy { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
