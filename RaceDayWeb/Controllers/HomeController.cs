using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Models;
using System.Diagnostics;
using RaceDayWeb.Helpers;

namespace RaceDayWeb.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            string role = HttpContext.Session.GetString(SessionKeys.Role);

            // logged-in users go straight to their own dashboard
            if (role == Roles.Organiser)
                return RedirectToAction("Index", "Organiser");

            if (role == Roles.Participant)
                return RedirectToAction("Index", "Participant");

            // everyone else sees the public welcome page
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        // shown for pages that are not found (404) and other error codes
        public IActionResult StatusPage(int code)
        {
            ViewBag.Code = code;
            return View();
        }
    }
}
