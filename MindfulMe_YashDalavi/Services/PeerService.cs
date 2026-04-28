using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MindfulMe_YashDalavi.DataAccess;
using MindfulMe_YashDalavi.Models;

namespace MindfulMe_YashDalavi.Services
{
    public class PeerService
    {
        private readonly DBHelper _db;

        private static readonly string[] AnonymousNames = {
            "SilentOwl", "BraveLion", "CalmRiver", "KindPhoenix", "WiseMoon",
            "GentleWave", "BoldEagle", "QuietStar", "StrongOak", "HopeSeeker",
            "LightKeeper", "MoonWalker", "SunChaser", "StormRider", "DreamWeaver"
        };

        public PeerService()
        {
            _db = new DBHelper();
        }

        public string GenerateAnonymousName()
        {
            Random rand = new Random();
            int index = rand.Next(AnonymousNames.Length);
            int suffix = rand.Next(100, 999);
            return AnonymousNames[index] + suffix;
        }

        public int CreatePost(PeerPost post)
        {
            if (post == null)
                throw new ArgumentNullException(nameof(post));

            if (string.IsNullOrWhiteSpace(post.UserId))
                throw new ArgumentException("User ID is required.");

            if (string.IsNullOrWhiteSpace(post.Content))
                throw new ArgumentException("Post content cannot be empty.");

            if (post.Content.Length > 1000)
                throw new ArgumentException("Post content exceeds 1000 characters.");

            if (string.IsNullOrWhiteSpace(post.AnonymousName))
                post.AnonymousName = GenerateAnonymousName();

            string query = @"
                INSERT INTO PeerPosts (UserId, AnonymousName, Content, MoodTag, 
                                       LikesCount, IsReported, IsHidden, PostedOn)
                VALUES (@UserId, @AnonymousName, @Content, @MoodTag, 
                        0, 0, 0, @PostedOn);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", post.UserId),
                new SqlParameter("@AnonymousName", post.AnonymousName),
                new SqlParameter("@Content", post.Content.Trim()),
                new SqlParameter("@MoodTag", (object)post.MoodTag ?? DBNull.Value),
                new SqlParameter("@PostedOn", DateTime.Now)
            };

            object newId = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(newId);
        }

        public List<PeerPost> GetAllPosts(string moodTag = null)
        {
            List<PeerPost> list = new List<PeerPost>();

            string query = @"
                SELECT p.PostId, p.UserId, p.AnonymousName, p.Content, p.MoodTag,
                       p.LikesCount, p.IsReported, p.IsHidden, p.PostedOn,
                       (SELECT COUNT(*) FROM PeerReplies WHERE PostId = p.PostId) AS RepliesCount
                FROM PeerPosts p
                WHERE p.IsHidden = 0
                  AND (@MoodTag IS NULL OR p.MoodTag = @MoodTag)
                ORDER BY p.PostedOn DESC;";

            SqlParameter[] parameters = {
                new SqlParameter("@MoodTag",
                    string.IsNullOrWhiteSpace(moodTag) ? (object)DBNull.Value : moodTag)
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToPost(row));
            }

            return list;
        }

        public PeerPost GetPostWithReplies(int postId)
        {
            if (postId <= 0)
                return null;

            string postQuery = @"
                SELECT p.PostId, p.UserId, p.AnonymousName, p.Content, p.MoodTag,
                       p.LikesCount, p.IsReported, p.IsHidden, p.PostedOn,
                       (SELECT COUNT(*) FROM PeerReplies WHERE PostId = p.PostId) AS RepliesCount
                FROM PeerPosts p
                WHERE p.PostId = @PostId AND p.IsHidden = 0;";

            SqlParameter[] parameters = {
                new SqlParameter("@PostId", postId)
            };

            DataTable dt = _db.ExecuteQuery(postQuery, parameters);

            if (dt.Rows.Count == 0)
                return null;

            PeerPost post = MapRowToPost(dt.Rows[0]);
            post.Replies = GetRepliesForPost(postId);
            return post;
        }

        public List<PeerReply> GetRepliesForPost(int postId)
        {
            List<PeerReply> list = new List<PeerReply>();

            string query = @"
                SELECT ReplyId, PostId, UserId, AnonymousName, Content, RepliedOn
                FROM PeerReplies
                WHERE PostId = @PostId
                ORDER BY RepliedOn ASC;";

            SqlParameter[] parameters = {
                new SqlParameter("@PostId", postId)
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new PeerReply
                {
                    ReplyId = Convert.ToInt32(row["ReplyId"]),
                    PostId = Convert.ToInt32(row["PostId"]),
                    UserId = row["UserId"].ToString(),
                    AnonymousName = row["AnonymousName"].ToString(),
                    Content = row["Content"].ToString(),
                    RepliedOn = Convert.ToDateTime(row["RepliedOn"])
                });
            }

            return list;
        }

        public int AddReply(PeerReply reply)
        {
            if (reply == null)
                throw new ArgumentNullException(nameof(reply));

            if (reply.PostId <= 0)
                throw new ArgumentException("Invalid post.");

            if (string.IsNullOrWhiteSpace(reply.UserId))
                throw new ArgumentException("User ID is required.");

            if (string.IsNullOrWhiteSpace(reply.Content))
                throw new ArgumentException("Reply cannot be empty.");

            if (reply.Content.Length > 500)
                throw new ArgumentException("Reply exceeds 500 characters.");

            if (string.IsNullOrWhiteSpace(reply.AnonymousName))
                reply.AnonymousName = GenerateAnonymousName();

            string query = @"
                INSERT INTO PeerReplies (PostId, UserId, AnonymousName, Content, RepliedOn)
                VALUES (@PostId, @UserId, @AnonymousName, @Content, @RepliedOn);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters = {
                new SqlParameter("@PostId", reply.PostId),
                new SqlParameter("@UserId", reply.UserId),
                new SqlParameter("@AnonymousName", reply.AnonymousName),
                new SqlParameter("@Content", reply.Content.Trim()),
                new SqlParameter("@RepliedOn", DateTime.Now)
            };

            object newId = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(newId);
        }

        public bool LikePost(int postId)
        {
            if (postId <= 0)
                return false;

            string query = @"
                UPDATE PeerPosts 
                SET LikesCount = LikesCount + 1 
                WHERE PostId = @PostId AND IsHidden = 0;";

            SqlParameter[] parameters = {
                new SqlParameter("@PostId", postId)
            };

            int rows = _db.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        public bool ReportPost(int postId)
        {
            if (postId <= 0)
                return false;

            string query = "UPDATE PeerPosts SET IsReported = 1 WHERE PostId = @PostId;";

            SqlParameter[] parameters = {
                new SqlParameter("@PostId", postId)
            };

            int rows = _db.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        public bool HidePost(int postId)
        {
            if (postId <= 0)
                return false;

            string query = "UPDATE PeerPosts SET IsHidden = 1 WHERE PostId = @PostId;";

            SqlParameter[] parameters = {
                new SqlParameter("@PostId", postId)
            };

            int rows = _db.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        public List<PeerPost> GetReportedPosts()
        {
            List<PeerPost> list = new List<PeerPost>();

            string query = @"
                SELECT p.PostId, p.UserId, p.AnonymousName, p.Content, p.MoodTag,
                       p.LikesCount, p.IsReported, p.IsHidden, p.PostedOn,
                       (SELECT COUNT(*) FROM PeerReplies WHERE PostId = p.PostId) AS RepliesCount
                FROM PeerPosts p
                WHERE p.IsReported = 1
                ORDER BY p.PostedOn DESC;";

            DataTable dt = _db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToPost(row));
            }

            return list;
        }

        private PeerPost MapRowToPost(DataRow row)
        {
            return new PeerPost
            {
                PostId = Convert.ToInt32(row["PostId"]),
                UserId = row["UserId"].ToString(),
                AnonymousName = row["AnonymousName"].ToString(),
                Content = row["Content"].ToString(),
                MoodTag = row["MoodTag"] == DBNull.Value ? string.Empty : row["MoodTag"].ToString(),
                LikesCount = Convert.ToInt32(row["LikesCount"]),
                IsReported = Convert.ToBoolean(row["IsReported"]),
                IsHidden = Convert.ToBoolean(row["IsHidden"]),
                PostedOn = Convert.ToDateTime(row["PostedOn"]),
                RepliesCount = Convert.ToInt32(row["RepliesCount"])
            };
        }
    }
}