using System;

namespace MindfulMe_YashDalavi.Models
{
    public class CounselorBooking
    {
        public int BookingId { get; set; }
        public string UserId { get; set; }
        public int CounselorId { get; set; }
        public DateTime BookingDate { get; set; }
        public DateTime SessionDate { get; set; }
        public string Status { get; set; }
        public decimal AmountPaid { get; set; }
        public string PaymentMethod { get; set; }
        public string Notes { get; set; }

        public string CounselorName { get; set; }
        public string CounselorSpecialization { get; set; }
    }
}