using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class CreateEmotionTrackDto
    {
        [Required(ErrorMessage = "Emotion is required")]
        [StringLength(50, ErrorMessage = "Emotion cannot exceed 50 characters")]
        public string Emotion { get; set; } = string.Empty;

        [StringLength(255, ErrorMessage = "Note cannot exceed 255 characters")]
        public string? Note { get; set; }

        [Range(1, 10, ErrorMessage = "Intensity level must be between 1 and 10")]
        public int? IntensityLevel { get; set; }
    }
}
