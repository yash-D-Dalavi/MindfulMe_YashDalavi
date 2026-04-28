using System;

namespace MindfulMe_YashDalavi.Models
{
    public class Counselor
    {
        public int CounselorId { get; set; }
        public string FullName { get; set; }
        public string Specialization { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public int ExperienceYears { get; set; }
        public decimal FeePerSession { get; set; }
        public string Bio { get; set; }
        public string ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}