using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers;

[RequireLogin(Roles.Organiser)]
public class EventsController : BaseController
{
    public EventsController(ApiService api) : base(api)
    {
    }

    // ---------- list ----------

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        ApiResponse response = await CallApi(HttpMethod.Get, "api/events/mine");
        if (!response.IsSuccess)
            return HandleFailure(response);

        List<EventItem> events = ReadJson<List<EventItem>>(response);
        return View(events);
    }

    // ---------- create ----------

    [HttpGet]
    public IActionResult Create()
    {
        EventFormViewModel model = new EventFormViewModel();
        model.EventDate = DateTime.Today.AddDays(30);
        model.EventType = "Run";
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EventFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var details = new
        {
            EventName = model.EventName,
            Description = model.Description,
            EventDate = model.EventDate,
            Location = model.Location,
            DistanceKm = model.DistanceKm,
            EventType = model.EventType
        };

        ApiResponse response = await CallApi(HttpMethod.Post, "api/events", details);
        if (!response.IsSuccess)
        {
            if (response.StatusCode == 401)
                return GoToLogin();

            ModelState.AddModelError("", _api.GetErrorMessage(response));
            return View(model);
        }

        EventItem created = ReadJson<EventItem>(response);

        // if a banner was chosen, send it to the API now that the event has an ID
        if (model.BannerFile != null && model.BannerFile.Length > 0)
        {
            string error = await UploadBanner(created.EventID, model.BannerFile);
            if (error != null)
            {
                TempData["Error"] = "The event was created, but the banner could not be uploaded: " + error;
                return RedirectToAction("Edit", new { id = created.EventID });
            }
        }

        TempData["Success"] = "Event created.";
        return RedirectToAction("Index");
    }

    // ---------- edit ----------

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        ApiResponse response = await CallApi(HttpMethod.Get, "api/events/" + id);
        if (!response.IsSuccess)
            return HandleFailure(response);

        EventItem ev = ReadJson<EventItem>(response);

        // an organiser may only edit their own events
        if (ev.OrganiserID != CurrentUserId())
            return RedirectToAction("AccessDenied", "Account");

        EventFormViewModel model = new EventFormViewModel();
        model.EventID = ev.EventID;
        model.EventName = ev.EventName;
        model.Description = ev.Description;
        model.EventDate = ev.EventDate;
        model.Location = ev.Location;
        model.DistanceKm = ev.DistanceKm;
        model.EventType = ev.EventType;
        model.BannerUrl = ev.BannerUrl;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EventFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var details = new
        {
            EventName = model.EventName,
            Description = model.Description,
            EventDate = model.EventDate,
            Location = model.Location,
            DistanceKm = model.DistanceKm,
            EventType = model.EventType
        };

        ApiResponse response = await CallApi(HttpMethod.Put, "api/events/" + model.EventID, details);
        if (!response.IsSuccess)
        {
            if (response.StatusCode == 401)
                return GoToLogin();

            ModelState.AddModelError("", _api.GetErrorMessage(response));
            return View(model);
        }

        // a new banner replaces the old one
        if (model.BannerFile != null && model.BannerFile.Length > 0)
        {
            string error = await UploadBanner(model.EventID, model.BannerFile);
            if (error != null)
            {
                TempData["Error"] = "The event was saved, but the banner could not be uploaded: " + error;
                return RedirectToAction("Edit", new { id = model.EventID });
            }
        }

        TempData["Success"] = "Event updated.";
        return RedirectToAction("Index");
    }

    // ---------- delete ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        ApiResponse response = await CallApi(HttpMethod.Delete, "api/events/" + id);

        if (response.StatusCode == 401)
            return GoToLogin();

        if (response.IsSuccess)
            TempData["Success"] = "Event deleted.";
        else
            TempData["Error"] = _api.GetErrorMessage(response);

        return RedirectToAction("Index");
    }

    // sends the banner to the API. Gives back an error message, or null if it worked
    private async Task<string> UploadBanner(int eventId, IFormFile file)
    {
        string cookie = HttpContext.Session.GetString(SessionKeys.ApiCookie);
        ApiResponse response = await _api.SendFileAsync("api/events/" + eventId + "/banner", file, cookie);

        if (response.IsSuccess)
            return null;

        return _api.GetErrorMessage(response);
    }
}