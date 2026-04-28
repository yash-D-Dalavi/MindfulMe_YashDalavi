using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MindfulMe_YashDalavi.Services;

namespace MindfulMe_YashDalavi.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        private readonly MoodService _moodService;
        private readonly BookingService _bookingService;
        private readonly ActivityService _activityService;

        public DashboardController()
        {
            _moodService = new MoodService();
            _bookingService = new BookingService();
            _activityService = new ActivityService();
        }

        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();

            ViewBag.UserEmail = User.Identity.Name;
            ViewBag.TodayMood = _moodService.GetTodayMood(userId);
            ViewBag.StreakCount = _moodService.GetStreakCount(userId);
            ViewBag.RecentMoods = _moodService.GetUserMoodEntries(userId, 7);
            ViewBag.MoodDistribution = _moodService.GetMoodDistribution(userId, 30);

            ViewBag.TotalMinutes = _activityService.GetTotalMinutesCompleted(userId);
            ViewBag.RecentCompletions = _activityService.GetUserCompletions(userId, 7);

            ViewBag.UserBookings = _bookingService.GetUserBookings(userId);

            return View();
        }
    }
}