using System.Web.Mvc;
using MindfulMe_YashDalavi.Services;

namespace MindfulMe_YashDalavi.Controllers
{
    public class HomeController : Controller
    {
        private readonly CounselorService _counselorService;
        private readonly ActivityService _activityService;

        public HomeController()
        {
            _counselorService = new CounselorService();
            _activityService = new ActivityService();
        }

        public ActionResult Index()
        {
            ViewBag.FeaturedCounselors = _counselorService.GetAllActiveCounselors();
            ViewBag.FeaturedActivities = _activityService.GetAllActivities();
            ViewBag.Title = "MindfulMe - Your Mental Wellness Companion";
            return View();
        }

        public ActionResult About()
        {
            ViewBag.Title = "About MindfulMe";
            ViewBag.Message = "Supporting student mental wellness through technology.";
            return View();
        }

        public ActionResult Contact()
        {
            ViewBag.Title = "Contact & Crisis Helplines";
            return View();
        }
    }
}