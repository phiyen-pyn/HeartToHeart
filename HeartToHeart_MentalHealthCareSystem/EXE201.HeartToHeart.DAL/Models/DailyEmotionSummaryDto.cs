using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class DailyEmotionSummaryDto
    {
        public DateTime Date { get; set; }
        public List<EmotionTrackDto> Emotions { get; set; } = new List<EmotionTrackDto>();
        public string? DominantEmotion { get; set; }
        public double? AverageIntensity { get; set; }
        public int TotalRecords { get; set; }
    }
}
