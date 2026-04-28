using System;
using System.Collections.Generic;

namespace MindfulMe_YashDalavi.Models
{
    public class PeerPost
    {
        public int PostId { get; set; }
        public string UserId { get; set; }
        public string AnonymousName { get; set; }
        public string Content { get; set; }
        public string MoodTag { get; set; }
        public int LikesCount { get; set; }
        public bool IsReported { get; set; }
        public bool IsHidden { get; set; }
        public DateTime PostedOn { get; set; }

        public List<PeerReply> Replies { get; set; }
        public int RepliesCount { get; set; }
    }
}