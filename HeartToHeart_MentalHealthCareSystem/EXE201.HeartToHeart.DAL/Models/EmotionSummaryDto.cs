using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class EmotionSummaryDto
    {
        public string Emotion { get; set; } = string.Empty;
        public int Count { get; set; }
        public double AverageIntensity { get; set; }
        public int? MaxIntensity { get; set; }
        public int? MinIntensity { get; set; }
        public DateTime LastRecorded { get; set; }
    }
}
