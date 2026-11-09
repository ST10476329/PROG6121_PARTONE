using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers;

[RequireLogin(Roles.Participant)]
public class ParticipantController : BaseController
{
    public ParticipantController(ApiService api) : base(api)
    {
    }

    // ---------- home: upcoming events with filters ----------

    [HttpGet]
    public async Task<IActionResult> Index(string search, string type, bool showPast = false)
    {
        // build the address with the filters the user chose (the API does the filtering)
        string url = "api/events?upcomingOnly=" + (showPast ? "false" : "true");

        if (!string.IsNullOrWhiteSpace(search))
            url = url + "&search=" + Uri.EscapeDataString(search);

        if (!string.IsNullOrWhiteSpace(type))
            url = url + "&type=" + Uri.EscapeDataString(type);

        ApiResponse response = await CallApi(HttpMethod.Get, url);
        if (!response.IsSuccess)
            return HandleFailure(response);

        // find the events this participant has already entered, so we can mark them
        List<int> enrolledIds = new List<int>();
        ApiResponse myResponse = await CallApi(HttpMethod.Get, "api/users/me/enrolments");
        if (myResponse.IsSuccess)
        {
            List<EnrolmentItem> mine = ReadJson<List<EnrolmentItem>>(myResponse);
            foreach (EnrolmentItem en in mine)
            {
                enrolledIds.Add(en.EventID);
            }
        }

        ParticipantHomeViewModel model = new ParticipantHomeViewModel();
        model.Events = ReadJson<List<EventItem>>(response);
        model.EnrolledEventIds = enrolledIds;
        model.Search = search;
        model.Type = type;
        model.ShowPast = showPast;
        return View(model);
    }

    // ---------- my events: everything I have enrolled for ----------

    [HttpGet]
    public async Task<IActionResult> MyEvents()
    {
        ApiResponse response = await CallApi(HttpMethod.Get, "api/users/me/enrolments");
        if (!response.IsSuccess)
            return HandleFailure(response);

        List<EnrolmentItem> enrolments = ReadJson<List<EnrolmentItem>>(response);
        return View(enrolments);
    }

    // cancel one of my enrolments
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        ApiResponse response = await CallApi(HttpMethod.Delete, "api/enrolments/" + id);

        if (response.StatusCode == 401)
            return GoToLogin();

        if (response.IsSuccess)
            TempData["Success"] = "Your enrolment has been cancelled.";
        else
            TempData["Error"] = _api.GetErrorMessage(response);

        return RedirectToAction("MyEvents");
    }

    // ---------- my results: my race history ----------

    [HttpGet]
    public async Task<IActionResult> MyResults()
    {
        ApiResponse response = await CallApi(HttpMethod.Get, "api/users/me/results");
        if (!response.IsSuccess)
            return HandleFailure(response);

        List<ResultItem> results = ReadJson<List<ResultItem>>(response);
        return View(results);
    }
}