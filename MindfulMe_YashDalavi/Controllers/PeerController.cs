using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MindfulMe_YashDalavi.Models;
using MindfulMe_YashDalavi.Services;

namespace MindfulMe_YashDalavi.Controllers
{
    [Authorize]
    public class PeerController : Controller
    {
        private readonly PeerService _peerService;

        public PeerController()
        {
            _peerService = new PeerService();
        }

        public ActionResult Index(string moodTag)
        {
            ViewBag.MoodTag = moodTag;
            ViewBag.Posts = _peerService.GetAllPosts(moodTag);
            return View();
        }

        public ActionResult Create()
        {
            ViewBag.SuggestedName = _peerService.GenerateAnonymousName();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(string content, string moodTag, string anonymousName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(content))
                {
                    TempData["ErrorMessage"] = "Post content cannot be empty.";
                    return RedirectToAction("Create");
                }

                var post = new PeerPost
                {
                    UserId = User.Identity.GetUserId(),
                    AnonymousName = string.IsNullOrWhiteSpace(anonymousName)
                        ? _peerService.GenerateAnonymousName()
                        : anonymousName,
                    Content = content,
                    MoodTag = moodTag
                };

                _peerService.CreatePost(post);
                TempData["SuccessMessage"] = "Your post is live on the wall. You're not alone. 💙";
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
                return RedirectToAction("Create");
            }
        }

        public ActionResult Details(int? id)
        {
            if (!id.HasValue || id.Value <= 0)
                return RedirectToAction("Index");

            var post = _peerService.GetPostWithReplies(id.Value);
            if (post == null)
            {
                TempData["ErrorMessage"] = "Post not found.";
                return RedirectToAction("Index");
            }

            ViewBag.SuggestedName = _peerService.GenerateAnonymousName();
            return View(post);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Reply(int postId, string content, string anonymousName)
        {
            try
            {
                var reply = new PeerReply
                {
                    PostId = postId,
                    UserId = User.Identity.GetUserId(),
                    AnonymousName = string.IsNullOrWhiteSpace(anonymousName)
                        ? _peerService.GenerateAnonymousName()
                        : anonymousName,
                    Content = content
                };

                _peerService.AddReply(reply);
                TempData["SuccessMessage"] = "Reply posted. Kindness matters. 🤝";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = postId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Like(int id)
        {
            _peerService.LikePost(id);
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Report(int id)
        {
            _peerService.ReportPost(id);
            TempData["SuccessMessage"] = "Post reported. Our team will review it.";
            return RedirectToAction("Index");
        }
    }
}