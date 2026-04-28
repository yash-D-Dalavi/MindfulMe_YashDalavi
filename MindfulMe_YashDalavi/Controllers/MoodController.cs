using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MindfulMe_YashDalavi.Models;
using MindfulMe_YashDalavi.Services;

namespace MindfulMe_YashDalavi.Controllers
{
    [Authorize]
    public class MoodController : Controller
    {
        private readonly MoodService _moodService;

        public MoodController()
        {
            _moodService = new MoodService();
        }

        public ActionResult Index()
        {
            string userId = User.Identity.GetUserId();

            ViewBag.TodayMood = _moodService.GetTodayMood(userId);
            ViewBag.StreakCount = _moodService.GetStreakCount(userId);
            ViewBag.RecentEntries = _moodService.GetUserMoodEntries(userId, 7);

            return View();
        }

        public ActionResult Add()
        {
            string userId = User.Identity.GetUserId();
            ViewBag.TodayMood = _moodService.GetTodayMood(userId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Add(int moodLevel, string moodEmoji, string note)
        {
            try
            {
                if (moodLevel < 1 || moodLevel > 5)
                {
                    TempData["ErrorMessage"] = "Please select a valid mood.";
                    return RedirectToAction("Add");
                }

                MoodEntry entry = new MoodEntry
                {
                    UserId = User.Identity.GetUserId(),
                    MoodLevel = moodLevel,
                    MoodEmoji = moodEmoji ?? "😐",
                    Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim()
                };

                _moodService.AddMoodEntry(entry);

                TempData["SuccessMessage"] = "Mood logged successfully! Keep up the streak. 🔥";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Something went wrong: " + ex.Message;
                return RedirectToAction("Add");
            }
        }

        public ActionResult History(int days = 30)
        {
            if (days != 7 && days != 30 && days != 90)
                days = 30;

            string userId = User.Identity.GetUserId();
            ViewBag.Days = days;
            ViewBag.Entries = _moodService.GetUserMoodEntries(userId, days);
            ViewBag.Distribution = _moodService.GetMoodDistribution(userId, days);

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                bool deleted = _moodService.DeleteMoodEntry(id, userId);

                if (deleted)
                    TempData["SuccessMessage"] = "Entry deleted.";
                else
                    TempData["ErrorMessage"] = "Could not delete entry.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction("History");
        }
    }
}