using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MindfulMe_YashDalavi.DataAccess;
using MindfulMe_YashDalavi.Models;

namespace MindfulMe_YashDalavi.Services
{
    public class ActivityService
    {
        private readonly DBHelper _db;

        public ActivityService()
        {
            _db = new DBHelper();
        }

        public List<Activity> GetAllActivities()
        {
            List<Activity> list = new List<Activity>();

            string query = @"
                SELECT ActivityId, Title, Category, DurationMinutes, 
                       Description, Instructions, IconEmoji, IsActive
                FROM Activities
                WHERE IsActive = 1
                ORDER BY Category, DurationMinutes;";

            DataTable dt = _db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToActivity(row));
            }

            return list;
        }

        public List<Activity> GetActivitiesByCategory(string category)
        {
            List<Activity> list = new List<Activity>();

            string query = @"
                SELECT ActivityId, Title, Category, DurationMinutes, 
                       Description, Instructions, IconEmoji, IsActive
                FROM Activities
                WHERE IsActive = 1
                  AND (@Category IS NULL OR Category = @Category)
                ORDER BY DurationMinutes;";

            SqlParameter[] parameters = {
                new SqlParameter("@Category",
                    string.IsNullOrWhiteSpace(category) ? (object)DBNull.Value : category)
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToActivity(row));
            }

            return list;
        }

        public Activity GetActivityById(int activityId)
        {
            if (activityId <= 0)
                return null;

            string query = @"
                SELECT ActivityId, Title, Category, DurationMinutes, 
                       Description, Instructions, IconEmoji, IsActive
                FROM Activities
                WHERE ActivityId = @ActivityId;";

            SqlParameter[] parameters = {
                new SqlParameter("@ActivityId", activityId)
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            return dt.Rows.Count == 0 ? null : MapRowToActivity(dt.Rows[0]);
        }

        public int LogCompletion(string userId, int activityId, string feedback)
        {
            if (string.IsNullOrWhiteSpace(userId))
                throw new ArgumentException("User ID is required.");

            if (activityId <= 0)
                throw new ArgumentException("Invalid activity.");

            string query = @"
                INSERT INTO ActivityCompletions (UserId, ActivityId, CompletedOn, Feedback)
                VALUES (@UserId, @ActivityId, @CompletedOn, @Feedback);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@ActivityId", activityId),
                new SqlParameter("@CompletedOn", DateTime.Now),
                new SqlParameter("@Feedback", (object)feedback ?? DBNull.Value)
            };

            object newId = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(newId);
        }

        public List<ActivityCompletion> GetUserCompletions(string userId, int days = 30)
        {
            List<ActivityCompletion> list = new List<ActivityCompletion>();

            if (string.IsNullOrWhiteSpace(userId))
                return list;

            string query = @"
                SELECT ac.CompletionId, ac.UserId, ac.ActivityId, ac.CompletedOn, ac.Feedback,
                       a.Title AS ActivityTitle, a.Category AS ActivityCategory
                FROM ActivityCompletions ac
                INNER JOIN Activities a ON ac.ActivityId = a.ActivityId
                WHERE ac.UserId = @UserId
                  AND ac.CompletedOn >= @FromDate
                ORDER BY ac.CompletedOn DESC;";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId),
                new SqlParameter("@FromDate", DateTime.Now.AddDays(-days))
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ActivityCompletion
                {
                    CompletionId = Convert.ToInt32(row["CompletionId"]),
                    UserId = row["UserId"].ToString(),
                    ActivityId = Convert.ToInt32(row["ActivityId"]),
                    CompletedOn = Convert.ToDateTime(row["CompletedOn"]),
                    Feedback = row["Feedback"] == DBNull.Value ? string.Empty : row["Feedback"].ToString(),
                    ActivityTitle = row["ActivityTitle"].ToString(),
                    ActivityCategory = row["ActivityCategory"].ToString()
                });
            }

            return list;
        }

        public int GetTotalMinutesCompleted(string userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
                return 0;

            string query = @"
                SELECT ISNULL(SUM(a.DurationMinutes), 0) AS TotalMinutes
                FROM ActivityCompletions ac
                INNER JOIN Activities a ON ac.ActivityId = a.ActivityId
                WHERE ac.UserId = @UserId;";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId)
            };

            object total = _db.ExecuteScalar(query, parameters);
            return total == null ? 0 : Convert.ToInt32(total);
        }

        public List<string> GetAllCategories()
        {
            List<string> list = new List<string>();

            string query = "SELECT DISTINCT Category FROM Activities WHERE IsActive = 1 ORDER BY Category;";

            DataTable dt = _db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(row["Category"].ToString());
            }

            return list;
        }

        private Activity MapRowToActivity(DataRow row)
        {
            return new Activity
            {
                ActivityId = Convert.ToInt32(row["ActivityId"]),
                Title = row["Title"].ToString(),
                Category = row["Category"].ToString(),
                DurationMinutes = Convert.ToInt32(row["DurationMinutes"]),
                Description = row["Description"].ToString(),
                Instructions = row["Instructions"].ToString(),
                IconEmoji = row["IconEmoji"] == DBNull.Value ? string.Empty : row["IconEmoji"].ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"])
            };
        }
    }
}