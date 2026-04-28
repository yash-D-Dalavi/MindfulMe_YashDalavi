using System;

namespace MindfulMe_YashDalavi.Models
{
    public class ActivityCompletion
    {
        public int CompletionId { get; set; }
        public string UserId { get; set; }
        public int ActivityId { get; set; }
        public DateTime CompletedOn { get; set; }
        public string Feedback { get; set; }

        public string ActivityTitle { get; set; }
        public string ActivityCategory { get; set; }
    }
}