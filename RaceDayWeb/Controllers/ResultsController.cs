using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers;

[RequireLogin(Roles.Organiser)]
public class ResultsController : BaseController
{
    public ResultsController(ApiService api) : base(api)
    {
    }

    // the enrolled participants of one event, each with a result form
    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        ApiResponse eventResponse = await CallApi(HttpMethod.Get, "api/events/" + eventId);
        if (!eventResponse.IsSuccess)
            return HandleFailure(eventResponse);

        ApiResponse enrolmentResponse = await CallApi(HttpMethod.Get, "api/events/" + eventId + "/enrolments");
        if (!enrolmentResponse.IsSuccess)
            return HandleFailure(enrolmentResponse);

        EnrolmentListViewModel model = new EnrolmentListViewModel();
        model.Event = ReadJson<EventItem>(eventResponse);
        model.Enrolments = ReadJson<List<EnrolmentItem>>(enrolmentResponse);
        return View(model);
    }

    // saves a result. Saving it publishes it, because participants see it straight away
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Save(ResultFormViewModel model)
    {
        // turn the typed time (like 01:45:30) into a TimeSpan
        TimeSpan? finishTime = null;
        if (!string.IsNullOrWhiteSpace(model.FinishTime))
        {
            TimeSpan parsed;
            if (!model.FinishTime.Contains(":") || !TimeSpan.TryParse(model.FinishTime, out parsed))
            {
                TempData["Error"] = "Enter the finish time like 01:45:30 (hours:minutes:seconds).";
                return RedirectToAction("Index", new { eventId = model.EventID });
            }
            finishTime = parsed;
        }

        var details = new
        {
            FinishTime = finishTime,
            Position = model.Position,
            Status = model.Status
        };

        // no result yet -> create one, otherwise update the one we have
        ApiResponse response;
        if (model.ResultID == null)
            response = await CallApi(HttpMethod.Post, "api/enrolments/" + model.EnrolmentID + "/results", details);
        else
            response = await CallApi(HttpMethod.Put, "api/results/" + model.ResultID.Value, details);

        if (response.StatusCode == 401)
            return GoToLogin();

        if (response.IsSuccess)
            TempData["Success"] = "Result saved and published.";
        else
            TempData["Error"] = _api.GetErrorMessage(response);

        return RedirectToAction("Index", new { eventId = model.EventID });
    }
}