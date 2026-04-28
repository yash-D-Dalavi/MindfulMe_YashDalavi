using System;

namespace MindfulMe_YashDalavi.Models
{
    public class MoodEntry
    {
        public int MoodId { get; set; }
        public string UserId { get; set; }
        public int MoodLevel { get; set; }
        public string MoodEmoji { get; set; }
        public string Note { get; set; }
        public DateTime EntryDate { get; set; }

        public string MoodLabel
        {
            get
            {
                switch (MoodLevel)
                {
                    case 1: return "Very Sad";
                    case 2: return "Sad";
                    case 3: return "Neutral";
                    case 4: return "Happy";
                    case 5: return "Very Happy";
                    default: return "Unknown";
                }
            }
        }
    }
}