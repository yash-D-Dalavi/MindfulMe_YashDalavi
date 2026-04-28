using System;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using MindfulMe_YashDalavi.Models;
using MindfulMe_YashDalavi.Services;

namespace MindfulMe_YashDalavi.Controllers
{
    [Authorize]
    public class BookingController : Controller
    {
        private readonly BookingService _bookingService;
        private readonly CounselorService _counselorService;

        public BookingController()
        {
            _bookingService = new BookingService();
            _counselorService = new CounselorService();
        }

        public ActionResult Book(int counselorId)
        {
            var counselor = _counselorService.GetCounselorById(counselorId);
            if (counselor == null || !counselor.IsActive)
            {
                TempData["ErrorMessage"] = "Counselor not available.";
                return RedirectToAction("Index", "Counselor");
            }

            ViewBag.Counselor = counselor;
            ViewBag.MinDate = DateTime.Now.AddDays(1).ToString("yyyy-MM-ddTHH:mm");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Book(int counselorId, DateTime sessionDate, string paymentMethod, string notes)
        {
            try
            {
                var counselor = _counselorService.GetCounselorById(counselorId);
                if (counselor == null)
                {
                    TempData["ErrorMessage"] = "Counselor not found.";
                    return RedirectToAction("Index", "Counselor");
                }

                var booking = new CounselorBooking
                {
                    UserId = User.Identity.GetUserId(),
                    CounselorId = counselorId,
                    SessionDate = sessionDate,
                    AmountPaid = counselor.FeePerSession,
                    PaymentMethod = paymentMethod,
                    Notes = notes
                };

                int bookingId = _bookingService.CreateBooking(booking);

                TempData["SuccessMessage"] = "Session booked successfully! Booking ID: #" + bookingId;
                return RedirectToAction("MyBookings");
            }
            catch (ArgumentException ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction("Book", new { counselorId = counselorId });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Booking failed: " + ex.Message;
                return RedirectToAction("Book", new { counselorId = counselorId });
            }
        }

        public ActionResult MyBookings()
        {
            string userId = User.Identity.GetUserId();
            ViewBag.Bookings = _bookingService.GetUserBookings(userId);
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Cancel(int id)
        {
            try
            {
                string userId = User.Identity.GetUserId();
                bool cancelled = _bookingService.CancelBooking(id, userId);

                if (cancelled)
                    TempData["SuccessMessage"] = "Booking cancelled successfully.";
                else
                    TempData["ErrorMessage"] = "Could not cancel this booking.";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Error: " + ex.Message;
            }

            return RedirectToAction("MyBookings");
        }
    }
}