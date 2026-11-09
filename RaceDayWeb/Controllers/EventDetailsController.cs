using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers;

[RequireLogin]
public class EventDetailsController : BaseController
{
    public EventDetailsController(ApiService api) : base(api)
    {
    }

    [HttpGet]
    public async Task<IActionResult> Index(int id)
    {

        ApiResponse eventResponse = await CallApi(HttpMethod.Get, "api/events/" + id);
        if (!eventResponse.IsSuccess)
            return HandleFailure(eventResponse);

        ApiResponse categoryResponse = await CallApi(HttpMethod.Get, "api/events/" + id + "/categories");
        if (!categoryResponse.IsSuccess)
            return HandleFailure(categoryResponse);

        EventDetailsViewModel model = new EventDetailsViewModel();
        model.Event = ReadJson<EventItem>(eventResponse);
        model.Categories = ReadJson<List<CategoryItem>>(categoryResponse);
        model.IsParticipant = HttpContext.Session.GetString(SessionKeys.Role) == Roles.Participant;

        // a participant may already be enrolled in this event
        if (model.IsParticipant)
        {
            ApiResponse myResponse = await CallApi(HttpMethod.Get, "api/users/me/enrolments");
            if (myResponse.IsSuccess)
            {
                List<EnrolmentItem> mine = ReadJson<List<EnrolmentItem>>(myResponse);
                model.MyEnrolment = mine.FirstOrDefault(e => e.EventID == id);
            }
        }

        return View(model);
    }

    // the participant enters the event with the category they chose
    [HttpPost]
    [ValidateAntiForgeryToken]
    [RequireLogin(Roles.Participant)]
    public async Task<IActionResult> Enrol(int eventId, int categoryId)
    {
        var details = new { CategoryID = categoryId };

        ApiResponse response = await CallApi(HttpMethod.Post, "api/events/" + eventId + "/enrolments", details);

        if (response.StatusCode == 401)
            return GoToLogin();

        if (response.IsSuccess)
            TempData["Success"] = "You are enrolled! Your race number is shown on this page.";
        else
            TempData["Error"] = _api.GetErrorMessage(response);

        return RedirectToAction("Index", new { id = eventId });
    }
}