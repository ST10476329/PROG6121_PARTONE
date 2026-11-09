using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers;

[RequireLogin(Roles.Organiser)]
public class OrganiserController : BaseController
{
    public OrganiserController(ApiService api) : base(api)
    {
    }

    public async Task<IActionResult> Index()
    {
        ApiResponse response = await CallApi(HttpMethod.Get, "api/events/mine");
        if (!response.IsSuccess)
            return HandleFailure(response);

        List<EventItem> events = ReadJson<List<EventItem>>(response);

        OrganiserDashboardViewModel model = new OrganiserDashboardViewModel();
        model.Events = events;
        model.TotalEvents = events.Count;

        foreach (EventItem e in events)
        {
            model.TotalEnrolments += e.EnrolmentCount;

            // the events come back soonest first, so the first upcoming one is the next event
            if (e.EventDate.Date >= DateTime.Today)
            {
                model.UpcomingEvents++;
                if (model.NextEventDate == null)
                    model.NextEventDate = e.EventDate;
            }
        }

        ViewBag.Name = HttpContext.Session.GetString(SessionKeys.FullName);
        return View(model);
    }
}