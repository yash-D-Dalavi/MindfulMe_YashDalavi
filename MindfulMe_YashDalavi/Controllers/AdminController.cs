using System;
using System.Web.Mvc;
using MindfulMe_YashDalavi.Models;
using MindfulMe_YashDalavi.Services;
using MindfulMe_YashDalavi.DataAccess;

namespace MindfulMe_YashDalavi.Controllers
{
    [Authorize]
    public class AdminController : Controller
    {
        private readonly CounselorService _counselorService;
        private readonly BookingService _bookingService;
        private readonly PeerService _peerService;
        private readonly DBHelper _db;

        public AdminController()
        {
            _counselorService = new CounselorService();
            _bookingService = new BookingService();
            _peerService = new PeerService();
            _db = new DBHelper();
        }

        public ActionResult Index()
        {
            var stats = _bookingService.GetBookingStats();
            ViewBag.BookingStats = stats;

            ViewBag.TotalCounselors = _counselorService.GetAllActiveCounselors().Count;
            ViewBag.TotalUsers = GetTotalUsersCount();
            ViewBag.TotalMoodEntries = GetTotalMoodEntriesCount();
            ViewBag.TotalPosts = GetTotalPostsCount();
            ViewBag.ReportedPosts = _peerService.GetReportedPosts();

            return View();
        }

        public ActionResult Counselors()
        {
            ViewBag.Counselors = _counselorService.GetAllActiveCounselors();
            return View();
        }

        public ActionResult AddCounselor()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AddCounselor(Counselor counselor)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(counselor.FullName) || string.IsNullOrWhiteSpace(counselor.Specialization))
                {
                    TempData["ErrorMessage"] = "Name and specialization are required.";
                    return RedirectToAction("AddCounselor");
                }

                _counselorService.AddCounselor(counselor);
                TempData["SuccessMessage"] = "Counselor added successfully!";
                return RedirectToAction("Counselors");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("AddCounselor");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteCounselor(int id)
        {
            try
            {
                _counselorService.DeleteCounselor(id);
                TempData["SuccessMessage"] = "Counselor removed successfully.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }
            return RedirectToAction("Counselors");
        }

        public ActionResult Reports()
        {
            ViewBag.ReportedPosts = _peerService.GetReportedPosts();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult HidePost(int id)
        {
            try
            {
                _peerService.HidePost(id);
                TempData["SuccessMessage"] = "Post has been hidden from the wall.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }
            return RedirectToAction("Reports");
        }

        private int GetTotalUsersCount()
        {
            try
            {
                var dt = _db.ExecuteQuery("SELECT COUNT(*) AS Count FROM AspNetUsers");
                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0]["Count"]);
            }
            catch { }
            return 0;
        }

        private int GetTotalMoodEntriesCount()
        {
            try
            {
                var dt = _db.ExecuteQuery("SELECT COUNT(*) AS Count FROM MoodEntries");
                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0]["Count"]);
            }
            catch { }
            return 0;
        }

        private int GetTotalPostsCount()
        {
            try
            {
                var dt = _db.ExecuteQuery("SELECT COUNT(*) AS Count FROM PeerPosts WHERE IsHidden = 0");
                if (dt.Rows.Count > 0)
                    return Convert.ToInt32(dt.Rows[0]["Count"]);
            }
            catch { }
            return 0;
        }
    }
}