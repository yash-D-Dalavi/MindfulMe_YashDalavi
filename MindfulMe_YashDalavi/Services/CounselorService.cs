using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MindfulMe_YashDalavi.DataAccess;
using MindfulMe_YashDalavi.Models;

namespace MindfulMe_YashDalavi.Services
{
    public class CounselorService
    {
        private readonly DBHelper _db;

        public CounselorService()
        {
            _db = new DBHelper();
        }

        public List<Counselor> GetAllActiveCounselors()
        {
            List<Counselor> list = new List<Counselor>();

            string query = @"
                SELECT CounselorId, FullName, Specialization, Email, Phone,
                       ExperienceYears, FeePerSession, Bio, ImageUrl, IsActive, CreatedOn
                FROM Counselors
                WHERE IsActive = 1
                ORDER BY ExperienceYears DESC;";

            DataTable dt = _db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToCounselor(row));
            }

            return list;
        }

        public List<Counselor> SearchCounselors(string keyword, string specialization)
        {
            List<Counselor> list = new List<Counselor>();

            string query = @"
                SELECT CounselorId, FullName, Specialization, Email, Phone,
                       ExperienceYears, FeePerSession, Bio, ImageUrl, IsActive, CreatedOn
                FROM Counselors
                WHERE IsActive = 1
                  AND (@Keyword IS NULL OR FullName LIKE '%' + @Keyword + '%' 
                       OR Specialization LIKE '%' + @Keyword + '%')
                  AND (@Specialization IS NULL OR Specialization = @Specialization)
                ORDER BY ExperienceYears DESC;";

            SqlParameter[] parameters = {
                new SqlParameter("@Keyword",
                    string.IsNullOrWhiteSpace(keyword) ? (object)DBNull.Value : keyword),
                new SqlParameter("@Specialization",
                    string.IsNullOrWhiteSpace(specialization) ? (object)DBNull.Value : specialization)
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToCounselor(row));
            }

            return list;
        }

        public Counselor GetCounselorById(int counselorId)
        {
            if (counselorId <= 0)
                return null;

            string query = @"
                SELECT CounselorId, FullName, Specialization, Email, Phone,
                       ExperienceYears, FeePerSession, Bio, ImageUrl, IsActive, CreatedOn
                FROM Counselors
                WHERE CounselorId = @CounselorId;";

            SqlParameter[] parameters = {
                new SqlParameter("@CounselorId", counselorId)
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            return dt.Rows.Count == 0 ? null : MapRowToCounselor(dt.Rows[0]);
        }

        public int AddCounselor(Counselor counselor)
        {
            if (counselor == null)
                throw new ArgumentNullException(nameof(counselor));

            if (string.IsNullOrWhiteSpace(counselor.FullName))
                throw new ArgumentException("Counselor name is required.");

            if (string.IsNullOrWhiteSpace(counselor.Email))
                throw new ArgumentException("Email is required.");

            string query = @"
                INSERT INTO Counselors 
                    (FullName, Specialization, Email, Phone, ExperienceYears, 
                     FeePerSession, Bio, ImageUrl, IsActive, CreatedOn)
                VALUES 
                    (@FullName, @Specialization, @Email, @Phone, @ExperienceYears, 
                     @FeePerSession, @Bio, @ImageUrl, @IsActive, @CreatedOn);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters = {
                new SqlParameter("@FullName", counselor.FullName),
                new SqlParameter("@Specialization", counselor.Specialization ?? string.Empty),
                new SqlParameter("@Email", counselor.Email),
                new SqlParameter("@Phone", counselor.Phone ?? string.Empty),
                new SqlParameter("@ExperienceYears", counselor.ExperienceYears),
                new SqlParameter("@FeePerSession", counselor.FeePerSession),
                new SqlParameter("@Bio", (object)counselor.Bio ?? DBNull.Value),
                new SqlParameter("@ImageUrl", (object)counselor.ImageUrl ?? DBNull.Value),
                new SqlParameter("@IsActive", true),
                new SqlParameter("@CreatedOn", DateTime.Now)
            };

            object newId = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(newId);
        }

        public bool UpdateCounselor(Counselor counselor)
        {
            if (counselor == null || counselor.CounselorId <= 0)
                return false;

            string query = @"
                UPDATE Counselors SET
                    FullName = @FullName,
                    Specialization = @Specialization,
                    Email = @Email,
                    Phone = @Phone,
                    ExperienceYears = @ExperienceYears,
                    FeePerSession = @FeePerSession,
                    Bio = @Bio,
                    ImageUrl = @ImageUrl,
                    IsActive = @IsActive
                WHERE CounselorId = @CounselorId;";

            SqlParameter[] parameters = {
                new SqlParameter("@CounselorId", counselor.CounselorId),
                new SqlParameter("@FullName", counselor.FullName),
                new SqlParameter("@Specialization", counselor.Specialization ?? string.Empty),
                new SqlParameter("@Email", counselor.Email),
                new SqlParameter("@Phone", counselor.Phone ?? string.Empty),
                new SqlParameter("@ExperienceYears", counselor.ExperienceYears),
                new SqlParameter("@FeePerSession", counselor.FeePerSession),
                new SqlParameter("@Bio", (object)counselor.Bio ?? DBNull.Value),
                new SqlParameter("@ImageUrl", (object)counselor.ImageUrl ?? DBNull.Value),
                new SqlParameter("@IsActive", counselor.IsActive)
            };

            int rows = _db.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        public bool DeleteCounselor(int counselorId)
        {
            if (counselorId <= 0)
                return false;

            string query = "UPDATE Counselors SET IsActive = 0 WHERE CounselorId = @CounselorId;";

            SqlParameter[] parameters = {
                new SqlParameter("@CounselorId", counselorId)
            };

            int rows = _db.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        public List<string> GetAllSpecializations()
        {
            List<string> list = new List<string>();

            string query = "SELECT DISTINCT Specialization FROM Counselors WHERE IsActive = 1 ORDER BY Specialization;";

            DataTable dt = _db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(row["Specialization"].ToString());
            }

            return list;
        }

        private Counselor MapRowToCounselor(DataRow row)
        {
            return new Counselor
            {
                CounselorId = Convert.ToInt32(row["CounselorId"]),
                FullName = row["FullName"].ToString(),
                Specialization = row["Specialization"].ToString(),
                Email = row["Email"].ToString(),
                Phone = row["Phone"].ToString(),
                ExperienceYears = Convert.ToInt32(row["ExperienceYears"]),
                FeePerSession = Convert.ToDecimal(row["FeePerSession"]),
                Bio = row["Bio"] == DBNull.Value ? string.Empty : row["Bio"].ToString(),
                ImageUrl = row["ImageUrl"] == DBNull.Value ? string.Empty : row["ImageUrl"].ToString(),
                IsActive = Convert.ToBoolean(row["IsActive"]),
                CreatedOn = Convert.ToDateTime(row["CreatedOn"])
            };
        }
    }
}