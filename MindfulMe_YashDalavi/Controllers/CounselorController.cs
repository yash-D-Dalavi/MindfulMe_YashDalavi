using System.Web.Mvc;
using MindfulMe_YashDalavi.Services;

namespace MindfulMe_YashDalavi.Controllers
{
    [Authorize]
    public class CounselorController : Controller
    {
        private readonly CounselorService _counselorService;

        public CounselorController()
        {
            _counselorService = new CounselorService();
        }

        public ActionResult Index(string keyword, string specialization)
        {
            ViewBag.Keyword = keyword;
            ViewBag.Specialization = specialization;
            ViewBag.Specializations = _counselorService.GetAllSpecializations();

            if (string.IsNullOrWhiteSpace(keyword) && string.IsNullOrWhiteSpace(specialization))
            {
                ViewBag.Counselors = _counselorService.GetAllActiveCounselors();
            }
            else
            {
                ViewBag.Counselors = _counselorService.SearchCounselors(keyword, specialization);
            }

            return View();
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
            {
                TempData["ErrorMessage"] = "Please select a counselor first.";
                return RedirectToAction("Index");
            }

            var counselor = _counselorService.GetCounselorById(id.Value);
            if (counselor == null || !counselor.IsActive)
            {
                TempData["ErrorMessage"] = "Counselor not found.";
                return RedirectToAction("Index");
            }

            return View(counselor);
        }
    }
}