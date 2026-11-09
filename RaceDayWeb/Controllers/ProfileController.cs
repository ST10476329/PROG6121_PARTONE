using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers;

[RequireLogin(Roles.Participant)]
public class ProfileController : BaseController
{
    public ProfileController(ApiService api) : base(api)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {

        ApiResponse response = await CallApi(HttpMethod.Get, "api/users/me");
        if (!response.IsSuccess)
            return HandleFailure(response);

        ApiUser user = ReadJson<ApiUser>(response);

        ProfileViewModel model = new ProfileViewModel();
        model.FullName = user.FullName;
        model.Email = user.Email;
        model.ProfilePictureUrl = user.ProfilePictureUrl;
        return View(model);
    }

    // saves the name and email
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Update(ProfileViewModel model)
    {
        model.ProfilePictureUrl = HttpContext.Session.GetString(SessionKeys.Picture);

        if (!ModelState.IsValid)
            return View("Index", model);

        var details = new { FullName = model.FullName, Email = model.Email };

        ApiResponse response = await CallApi(HttpMethod.Put, "api/users/me", details);
        if (response.StatusCode == 401)
            return GoToLogin();

        if (!response.IsSuccess)
        {
            // for example: the email already belongs to someone else
            ModelState.AddModelError("", _api.GetErrorMessage(response));
            return View("Index", model);
        }

        // keep the name in the navbar up to date
        ApiUser user = ReadJson<ApiUser>(response);
        HttpContext.Session.SetString(SessionKeys.FullName, user.FullName);

        TempData["Success"] = "Your profile has been updated.";
        return RedirectToAction("Index");
    }

    // sends the picture to the API
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UploadPicture(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            TempData["Error"] = "Please choose an image first.";
            return RedirectToAction("Index");
        }

        string cookie = HttpContext.Session.GetString(SessionKeys.ApiCookie);
        ApiResponse response = await _api.SendFileAsync("api/users/me/picture", file, cookie);

        if (response.StatusCode == 401)
            return GoToLogin();

        if (!response.IsSuccess)
        {
            TempData["Error"] = _api.GetErrorMessage(response);
            return RedirectToAction("Index");
        }

        // remember the new picture so the navbar shows it
        ImageUploadResult result = ReadJson<ImageUploadResult>(response);
        HttpContext.Session.SetString(SessionKeys.Picture, result.Url);

        TempData["Success"] = "Profile picture updated.";
        return RedirectToAction("Index");
    }
}