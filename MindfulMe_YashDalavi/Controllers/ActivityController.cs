using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MindfulMe_YashDalavi.Services;

namespace MindfulMe_YashDalavi.Controllers
{
    [Authorize]
    public class ActivityController : Controller
    {
        private readonly ActivityService _activityService;

        public ActivityController()
        {
            _activityService = new ActivityService();
        }

        public ActionResult Index(string category)
        {
            ViewBag.SelectedCategory = category;
            ViewBag.Categories = _activityService.GetAllCategories();

            if (string.IsNullOrWhiteSpace(category))
                ViewBag.Activities = _activityService.GetAllActivities();
            else
                ViewBag.Activities = _activityService.GetActivitiesByCategory(category);

            string userId = User.Identity.GetUserId();
            ViewBag.TotalMinutes = _activityService.GetTotalMinutesCompleted(userId);

            return View();
        }

        public ActionResult Play(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                TempData["ErrorMessage"] = "Please select an activity first.";
                return RedirectToAction("Index");
            }

            var activity = _activityService.GetActivityById(id.Value);
            if (activity == null)
            {
                TempData["ErrorMessage"] = "Activity not found.";
                return RedirectToAction("Index");
            }
            return View(activity);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Complete(int activityId, string feedback)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                _activityService.LogCompletion(userId, activityId, feedback);
                TempData["SuccessMessage"] = "Activity completed! Great job taking care of yourself. 🌟";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}