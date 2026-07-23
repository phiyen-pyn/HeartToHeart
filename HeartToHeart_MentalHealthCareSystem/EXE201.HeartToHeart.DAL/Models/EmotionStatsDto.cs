using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.DAL.Models
{
    public class EmotionStatsDto
    {
        public int TotalRecords { get; set; }
        public List<EmotionSummaryDto> EmotionBreakdown { get; set; } = new List<EmotionSummaryDto>();
        public double OverallAverageIntensity { get; set; }
        public string? MostFrequentEmotion { get; set; }
        public DateTime? LastRecorded { get; set; }
        public List<DailyEmotionSummaryDto> DailySummary { get; set; } = new List<DailyEmotionSummaryDto>();
    }
}
