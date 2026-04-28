using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MindfulMe_YashDalavi.DataAccess;
using MindfulMe_YashDalavi.Models;

namespace MindfulMe_YashDalavi.Services
{
    public class MoodService
    {
        private readonly DBHelper _db;

        public MoodService()
        {
            _db = new DBHelper();
        }

        public int AddMoodEntry(MoodEntry entry)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            if (entry.MoodLevel < 1 || entry.MoodLevel > 5)
                throw new ArgumentException("MoodLevel must be between 1 and 5.");

            string query = @"
                INSERT INTO MoodEntries (UserId, MoodLevel, MoodEmoji, Note, EntryDate)
                VALUES (@UserId, @MoodLevel, @MoodEmoji, @Note, @EntryDate);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", entry.UserId ?? (object)DBNull.Value),
                new SqlParameter("@MoodLevel", entry.MoodLevel),
                new SqlParameter("@MoodEmoji", entry.MoodEmoji ?? string.Empty),
                new SqlParameter("@Note", (object)entry.Note ?? DBNull.Value),
                new SqlParameter("@EntryDate", DateTime.Now)
            };

            object newId = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(newId);
        }

        public List<MoodEntry> GetUserMoodEntries(string userId, int days = 30)
        {
            List<MoodEntry> list = new List<MoodEntry>();

            if (string.IsNullOrWhiteSpace(userId))
                return list;

            string query = @"
                SELECT MoodId, UserId, MoodLevel, MoodEmoji, Note, EntryDate
                FROM MoodEntries
                WHERE UserId = @UserId
                  AND EntryDate >= @FromDate
                ORDER BY EntryDate DESC;";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@FromDate", DateTime.Now.AddDays(-days))
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new MoodEntry
                {
                    MoodId = Convert.ToInt32(row["MoodId"]),
                    UserId = row["UserId"].ToString(),
                    MoodLevel = Convert.ToInt32(row["MoodLevel"]),
                    MoodEmoji = row["MoodEmoji"].ToString(),
                    Note = row["Note"] == DBNull.Value ? string.Empty : row["Note"].ToString(),
                    EntryDate = Convert.ToDateTime(row["EntryDate"])
                });
            }

            return list;
        }

        public MoodEntry GetTodayMood(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return null;

            string query = @"
                SELECT TOP 1 MoodId, UserId, MoodLevel, MoodEmoji, Note, EntryDate
                FROM MoodEntries
                WHERE UserId = @UserId
                  AND CAST(EntryDate AS DATE) = CAST(GETDATE() AS DATE)
                ORDER BY EntryDate DESC;";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId)
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            if (dt.Rows.Count == 0)
                return null;

            DataRow row = dt.Rows[0];
            return new MoodEntry
            {
                MoodId = Convert.ToInt32(row["MoodId"]),
                UserId = row["UserId"].ToString(),
                MoodLevel = Convert.ToInt32(row["MoodLevel"]),
                MoodEmoji = row["MoodEmoji"].ToString(),
                Note = row["Note"] == DBNull.Value ? string.Empty : row["Note"].ToString(),
                EntryDate = Convert.ToDateTime(row["EntryDate"])
            };
        }

        public int GetStreakCount(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            string query = @"
                WITH DailyLogs AS (
                    SELECT DISTINCT CAST(EntryDate AS DATE) AS LogDate
                    FROM MoodEntries
                    WHERE UserId = @UserId
                ),
                Ranked AS (
                    SELECT LogDate,
                           ROW_NUMBER() OVER (ORDER BY LogDate DESC) AS rn
                    FROM DailyLogs
                )
                SELECT COUNT(*) FROM Ranked
                WHERE LogDate = DATEADD(DAY, -(rn - 1), CAST(GETDATE() AS DATE));";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId)
            };

            object count = _db.ExecuteScalar(query, parameters);
            return count == null ? 0 : Convert.ToInt32(count);
        }

        public bool DeleteMoodEntry(int moodId, string userId)
        {
            string query = @"
                DELETE FROM MoodEntries
                WHERE MoodId = @MoodId AND UserId = @UserId;";

            SqlParameter[] parameters = {
                new SqlParameter("@MoodId", moodId),
                new SqlParameter("@UserId", userId)
            };

            int rows = _db.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        public Dictionary<string, int> GetMoodDistribution(string userId, int days = 30)
        {
            Dictionary<string, int> distribution = new Dictionary<string, int>
            {
                { "Very Sad", 0 },
                { "Sad", 0 },
                { "Neutral", 0 },
                { "Happy", 0 },
                { "Very Happy", 0 }
            };

            if (string.IsNullOrWhiteSpace(userId))
                return distribution;

            string query = @"
                SELECT MoodLevel, COUNT(*) AS Total
                FROM MoodEntries
                WHERE UserId = @UserId AND EntryDate >= @FromDate
                GROUP BY MoodLevel;";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@FromDate", DateTime.Now.AddDays(-days))
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                int level = Convert.ToInt32(row["MoodLevel"]);
                int count = Convert.ToInt32(row["Total"]);

                switch (level)
                {
                    case 1: distribution["Very Sad"] = count; break;
                    case 2: distribution["Sad"] = count; break;
                    case 3: distribution["Neutral"] = count; break;
                    case 4: distribution["Happy"] = count; break;
                    case 5: distribution["Very Happy"] = count; break;
                }
            }

            return distribution;
        }
    }
}