using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EXE201.HeartToHeart.Common.Constants
{
    public static class Emotions
    {
        public static readonly List<string> AvailableEmotions = new()
        {
            "Happy", "Sad", "Angry", "Anxious", "Excited", "Calm", "Frustrated",
            "Content", "Overwhelmed", "Peaceful", "Stressed", "Joyful", "Depressed",
            "Grateful", "Worried", "Confident", "Lonely", "Energetic", "Tired", "Hopeful"
        };

        public static bool IsValidEmotion(string emotion)
        {
            return AvailableEmotions.Contains(emotion, StringComparer.OrdinalIgnoreCase);
        }

        public static string GetNormalizedEmotion(string emotion)
        {
            return AvailableEmotions.FirstOrDefault(e =>
                string.Equals(e, emotion, StringComparison.OrdinalIgnoreCase)) ?? emotion;
        }
    }
}
