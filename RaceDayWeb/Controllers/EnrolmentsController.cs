using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers;

[RequireLogin(Roles.Organiser)]
public class EnrolmentsController : BaseController
{
    public EnrolmentsController(ApiService api) : base(api)
    {
    }

    // everyone enrolled in one event
    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        ApiResponse eventResponse = await CallApi(HttpMethod.Get, "api/events/" + eventId);
        if (!eventResponse.IsSuccess)
            return HandleFailure(eventResponse);

        // the API only allows the owner of the event to see this (403 if it is not your event)
        ApiResponse enrolmentResponse = await CallApi(HttpMethod.Get, "api/events/" + eventId + "/enrolments");
        if (!enrolmentResponse.IsSuccess)
            return HandleFailure(enrolmentResponse);

        EnrolmentListViewModel model = new EnrolmentListViewModel();
        model.Event = ReadJson<EventItem>(eventResponse);
        model.Enrolments = ReadJson<List<EnrolmentItem>>(enrolmentResponse);
        return View(model);
    }
}