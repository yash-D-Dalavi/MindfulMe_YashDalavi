using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using MindfulMe_YashDalavi.DataAccess;
using MindfulMe_YashDalavi.Models;

namespace MindfulMe_YashDalavi.Services
{
    public class BookingService
    {
        private readonly DBHelper _db;

        private static readonly string[] ValidStatuses =
            { "Pending", "Confirmed", "Completed", "Cancelled" };

        private static readonly string[] ValidPaymentMethods =
            { "UPI", "Card", "NetBanking", "Wallet", "Cash" };

        public BookingService()
        {
            _db = new DBHelper();
        }

        public int CreateBooking(CounselorBooking booking)
        {
            if (booking == null)
                throw new ArgumentNullException(nameof(booking));

            if (string.IsNullOrWhiteSpace(booking.UserId))
                throw new ArgumentException("User ID is required.");

            if (booking.CounselorId <= 0)
                throw new ArgumentException("Invalid counselor selected.");

            if (booking.SessionDate < DateTime.Now)
                throw new ArgumentException("Session date cannot be in the past.");

            if (booking.AmountPaid <= 0)
                throw new ArgumentException("Amount must be greater than zero.");

            if (Array.IndexOf(ValidPaymentMethods, booking.PaymentMethod) < 0)
                throw new ArgumentException("Invalid payment method.");

            string query = @"
                INSERT INTO CounselorBookings
                    (UserId, CounselorId, BookingDate, SessionDate, Status, 
                     AmountPaid, PaymentMethod, Notes)
                VALUES
                    (@UserId, @CounselorId, @BookingDate, @SessionDate, @Status, 
                     @AmountPaid, @PaymentMethod, @Notes);
                SELECT CAST(SCOPE_IDENTITY() AS INT);";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", booking.UserId),
                new SqlParameter("@CounselorId", booking.CounselorId),
                new SqlParameter("@BookingDate", DateTime.Now),
                new SqlParameter("@SessionDate", booking.SessionDate),
                new SqlParameter("@Status", "Pending"),
                new SqlParameter("@AmountPaid", booking.AmountPaid),
                new SqlParameter("@PaymentMethod", booking.PaymentMethod),
                new SqlParameter("@Notes", (object)booking.Notes ?? DBNull.Value)
            };

            object newId = _db.ExecuteScalar(query, parameters);
            return Convert.ToInt32(newId);
        }

        public List<CounselorBooking> GetUserBookings(string userId)
        {
            List<CounselorBooking> list = new List<CounselorBooking>();

            if (string.IsNullOrWhiteSpace(userId))
                return list;

            string query = @"
                SELECT b.BookingId, b.UserId, b.CounselorId, b.BookingDate, 
                       b.SessionDate, b.Status, b.AmountPaid, b.PaymentMethod, b.Notes,
                       c.FullName AS CounselorName, c.Specialization AS CounselorSpecialization
                FROM CounselorBookings b
                INNER JOIN Counselors c ON b.CounselorId = c.CounselorId
                WHERE b.UserId = @UserId
                ORDER BY b.SessionDate DESC;";

            SqlParameter[] parameters = {
                new SqlParameter("@UserId", userId)
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToBooking(row));
            }

            return list;
        }

        public List<CounselorBooking> GetAllBookings()
        {
            List<CounselorBooking> list = new List<CounselorBooking>();

            string query = @"
                SELECT b.BookingId, b.UserId, b.CounselorId, b.BookingDate, 
                       b.SessionDate, b.Status, b.AmountPaid, b.PaymentMethod, b.Notes,
                       c.FullName AS CounselorName, c.Specialization AS CounselorSpecialization
                FROM CounselorBookings b
                INNER JOIN Counselors c ON b.CounselorId = c.CounselorId
                ORDER BY b.BookingDate DESC;";

            DataTable dt = _db.ExecuteQuery(query);

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapRowToBooking(row));
            }

            return list;
        }

        public CounselorBooking GetBookingById(int bookingId)
        {
            if (bookingId <= 0)
                return null;

            string query = @"
                SELECT b.BookingId, b.UserId, b.CounselorId, b.BookingDate, 
                       b.SessionDate, b.Status, b.AmountPaid, b.PaymentMethod, b.Notes,
                       c.FullName AS CounselorName, c.Specialization AS CounselorSpecialization
                FROM CounselorBookings b
                INNER JOIN Counselors c ON b.CounselorId = c.CounselorId
                WHERE b.BookingId = @BookingId;";

            SqlParameter[] parameters = {
                new SqlParameter("@BookingId", bookingId)
            };

            DataTable dt = _db.ExecuteQuery(query, parameters);
            return dt.Rows.Count == 0 ? null : MapRowToBooking(dt.Rows[0]);
        }

        public bool UpdateStatus(int bookingId, string newStatus)
        {
            if (bookingId <= 0)
                return false;

            if (Array.IndexOf(ValidStatuses, newStatus) < 0)
                throw new ArgumentException("Invalid status value.");

            string query = @"
                UPDATE CounselorBookings 
                SET Status = @Status 
                WHERE BookingId = @BookingId;";

            SqlParameter[] parameters = {
                new SqlParameter("@Status", newStatus),
                new SqlParameter("@BookingId", bookingId)
            };

            int rows = _db.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        public bool CancelBooking(int bookingId, string userId)
        {
            if (bookingId <= 0 || string.IsNullOrWhiteSpace(userId))
                return false;

            string query = @"
                UPDATE CounselorBookings 
                SET Status = 'Cancelled' 
                WHERE BookingId = @BookingId 
                  AND UserId = @UserId 
                  AND Status IN ('Pending', 'Confirmed');";

            SqlParameter[] parameters = {
                new SqlParameter("@BookingId", bookingId),
                new SqlParameter("@UserId", userId)
            };

            int rows = _db.ExecuteNonQuery(query, parameters);
            return rows > 0;
        }

        public Dictionary<string, decimal> GetBookingStats()
        {
            Dictionary<string, decimal> stats = new Dictionary<string, decimal>
            {
                { "TotalBookings", 0 },
                { "TotalRevenue", 0 },
                { "PendingCount", 0 },
                { "CompletedCount", 0 }
            };

            string query = @"
                SELECT 
                    COUNT(*) AS TotalBookings,
                    ISNULL(SUM(CASE WHEN Status = 'Completed' THEN AmountPaid ELSE 0 END), 0) AS TotalRevenue,
                    SUM(CASE WHEN Status = 'Pending' THEN 1 ELSE 0 END) AS PendingCount,
                    SUM(CASE WHEN Status = 'Completed' THEN 1 ELSE 0 END) AS CompletedCount
                FROM CounselorBookings;";

            DataTable dt = _db.ExecuteQuery(query);

            if (dt.Rows.Count > 0)
            {
                DataRow row = dt.Rows[0];
                stats["TotalBookings"] = Convert.ToDecimal(row["TotalBookings"]);
                stats["TotalRevenue"] = Convert.ToDecimal(row["TotalRevenue"]);
                stats["PendingCount"] = Convert.ToDecimal(row["PendingCount"]);
                stats["CompletedCount"] = Convert.ToDecimal(row["CompletedCount"]);
            }

            return stats;
        }

        private CounselorBooking MapRowToBooking(DataRow row)
        {
            return new CounselorBooking
            {
                BookingId = Convert.ToInt32(row["BookingId"]),
                UserId = row["UserId"].ToString(),
                CounselorId = Convert.ToInt32(row["CounselorId"]),
                BookingDate = Convert.ToDateTime(row["BookingDate"]),
                SessionDate = Convert.ToDateTime(row["SessionDate"]),
                Status = row["Status"].ToString(),
                AmountPaid = Convert.ToDecimal(row["AmountPaid"]),
                PaymentMethod = row["PaymentMethod"].ToString(),
                Notes = row["Notes"] == DBNull.Value ? string.Empty : row["Notes"].ToString(),
                CounselorName = row["CounselorName"].ToString(),
                CounselorSpecialization = row["CounselorSpecialization"].ToString()
            };
        }
    }
}