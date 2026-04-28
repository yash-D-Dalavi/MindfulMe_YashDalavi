namespace MindfulMe_YashDalavi.Models
{
    public class Activity
    {
        public int ActivityId { get; set; }
        public string Title { get; set; }
        public string Category { get; set; }
        public int DurationMinutes { get; set; }
        public string Description { get; set; }
        public string Instructions { get; set; }
        public string IconEmoji { get; set; }
        public bool IsActive { get; set; }
    }
}