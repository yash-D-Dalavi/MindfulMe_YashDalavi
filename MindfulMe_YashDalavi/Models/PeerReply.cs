using System;

namespace MindfulMe_YashDalavi.Models
{
    public class PeerReply
    {
        public int ReplyId { get; set; }
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string AnonymousName { get; set; }
        public string Content { get; set; }
        public DateTime RepliedOn { get; set; }
    }
}