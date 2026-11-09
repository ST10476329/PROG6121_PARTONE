using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers;

public class BaseController : Controller
{
    protected readonly ApiService _api;

    public BaseController(ApiService api)
    {
        _api = api;
    }

    // sends a request to the API using the logged-in user's saved API cookie
    protected async Task<ApiResponse> CallApi(HttpMethod method, string url, object body = null)
    {
        string cookie = HttpContext.Session.GetString(SessionKeys.ApiCookie);
        return await _api.SendAsync(method, url, body, cookie);
    }

    // turns the JSON from the API into C# objects (T is just "whatever type we ask for")
    protected T ReadJson<T>(ApiResponse response)
    {
        return JsonSerializer.Deserialize<T>(response.Body, ApiService.JsonOptions);
    }

    // the API session ended, so the user must log in again
    protected IActionResult GoToLogin()
    {
        HttpContext.Session.Clear();
        TempData["Error"] = "Your session has ended. Please log in again.";
        return RedirectToAction("Login", "Account");
    }

    // what to show when the API did not answer with success (for pages that show data)
    protected IActionResult HandleFailure(ApiResponse response)
    {
        if (response.StatusCode == 401)
            return GoToLogin();

        if (response.StatusCode == 403)
            return RedirectToAction("AccessDenied", "Account");

        ViewBag.Message = _api.GetErrorMessage(response);
        return View("ApiError");
    }

    // the ID of the logged-in user
    protected int CurrentUserId()
    {
        return HttpContext.Session.GetInt32(SessionKeys.UserId).Value;
    }
}