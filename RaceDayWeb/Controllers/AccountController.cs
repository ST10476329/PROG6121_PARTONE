using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Models;
using RaceDayWeb.Services;
using System.Text.Json;

namespace RaceDayWeb.Controllers;

public class AccountController : Controller
{
    private readonly ApiService _api;

    public AccountController(ApiService api)
    {
        _api = api;
    }

    // ---------- register ----------

    [HttpGet]
    public IActionResult Register()
    {

        // someone who is already logged in has no reason to see this page
        if (HttpContext.Session.GetString(SessionKeys.Role) != null)
            return RedirectToAction("Index", "Home");

        RegisterViewModel model = new RegisterViewModel();
        model.Role = Roles.Participant;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        // the details we send to the API (they match its RegisterDto)
        var details = new
        {
            FullName = model.FullName,
            Email = model.Email,
            Password = model.Password,
            Role = model.Role,
            OrganiserCode = model.OrganiserCode
        };

        ApiResponse response = await _api.SendAsync(HttpMethod.Post, "api/auth/register", details, null);

        if (!response.IsSuccess)
        {
            // for example: email already used, or the organiser code was wrong
            ModelState.AddModelError("", _api.GetErrorMessage(response));
            return View(model);
        }

        TempData["Success"] = "Account created. You can now log in.";
        return RedirectToAction("Login");
    }

    // ---------- login ----------

    [HttpGet]
    public IActionResult Login()
    {

        if (HttpContext.Session.GetString(SessionKeys.Role) != null)
            return RedirectToAction("Index", "Home");

        return View(new LoginViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var details = new { Email = model.Email, Password = model.Password };

        ApiResponse response = await _api.SendAsync(HttpMethod.Post, "api/auth/login", details, null);

        if (!response.IsSuccess)
        {
            ModelState.AddModelError("", _api.GetErrorMessage(response));
            return View(model);
        }

        // the API started a session for this user and sent back its cookie.
        // without that cookie, no later request would be recognised
        if (response.SessionCookie == null)
        {
            ModelState.AddModelError("", "Login worked, but the API did not start a session.");
            return View(model);
        }

        LoginApiResult result = JsonSerializer.Deserialize<LoginApiResult>(response.Body, ApiService.JsonOptions);

        // start a fresh MVC session and remember who this is
        HttpContext.Session.Clear();
        HttpContext.Session.SetString(SessionKeys.ApiCookie, response.SessionCookie);
        HttpContext.Session.SetInt32(SessionKeys.UserId, result.User.UserID);
        HttpContext.Session.SetString(SessionKeys.Role, result.User.Role);
        HttpContext.Session.SetString(SessionKeys.FullName, result.User.FullName);

        if (result.User.ProfilePictureUrl != null)
            HttpContext.Session.SetString(SessionKeys.Picture, result.User.ProfilePictureUrl);

        // Home sends each role to its own dashboard
        return RedirectToAction("Index", "Home");
    }

    // ---------- logout ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        // end the session on the API side too
        string cookie = HttpContext.Session.GetString(SessionKeys.ApiCookie);
        if (cookie != null)
        {
            await _api.SendAsync(HttpMethod.Post, "api/auth/logout", null, cookie);
        }

        // then clear the MVC session
        HttpContext.Session.Clear();

        TempData["Success"] = "You have been logged out.";
        return RedirectToAction("Login");
    }

    // ---------- wrong role ----------

    [HttpGet]
    public IActionResult AccessDenied()
    {

        return View();
    }
}