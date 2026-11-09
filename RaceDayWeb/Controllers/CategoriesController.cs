using Microsoft.AspNetCore.Mvc;
using RaceDayWeb.Helpers;
using RaceDayWeb.Models;
using RaceDayWeb.Services;

namespace RaceDayWeb.Controllers;

[RequireLogin(Roles.Organiser)]
public class CategoriesController : BaseController
{
    public CategoriesController(ApiService api) : base(api)
    {
    }

    // ---------- list the categories of one event ----------

    [HttpGet]
    public async Task<IActionResult> Index(int eventId)
    {
        ApiResponse eventResponse = await CallApi(HttpMethod.Get, "api/events/" + eventId);
        if (!eventResponse.IsSuccess)
            return HandleFailure(eventResponse);

        ApiResponse categoryResponse = await CallApi(HttpMethod.Get, "api/events/" + eventId + "/categories");
        if (!categoryResponse.IsSuccess)
            return HandleFailure(categoryResponse);

        CategoryListViewModel model = new CategoryListViewModel();
        model.Event = ReadJson<EventItem>(eventResponse);
        model.Categories = ReadJson<List<CategoryItem>>(categoryResponse);

        // an organiser may only manage the categories of their own events
        if (model.Event.OrganiserID != CurrentUserId())
            return RedirectToAction("AccessDenied", "Account");

        return View(model);
    }

    // ---------- add ----------

    [HttpGet]
    public IActionResult Create(int eventId)
    {
        CategoryFormViewModel model = new CategoryFormViewModel();
        model.EventID = eventId;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var details = new
        {
            CategoryName = model.CategoryName,
            DistanceKm = model.DistanceKm,
            EntryFee = model.EntryFee,
            MinAge = model.MinAge
        };

        ApiResponse response = await CallApi(HttpMethod.Post, "api/events/" + model.EventID + "/categories", details);
        if (!response.IsSuccess)
        {
            if (response.StatusCode == 401)
                return GoToLogin();

            ModelState.AddModelError("", _api.GetErrorMessage(response));
            return View(model);
        }

        TempData["Success"] = "Category added.";
        return RedirectToAction("Index", new { eventId = model.EventID });
    }

    // ---------- edit ----------

    [HttpGet]
    public async Task<IActionResult> Edit(int eventId, int id)
    {
        // the API has no "get one category", so we get the event's list and pick ours
        ApiResponse response = await CallApi(HttpMethod.Get, "api/events/" + eventId + "/categories");
        if (!response.IsSuccess)
            return HandleFailure(response);

        List<CategoryItem> categories = ReadJson<List<CategoryItem>>(response);
        CategoryItem category = categories.FirstOrDefault(c => c.CategoryID == id);
        if (category == null)
            return NotFound();

        CategoryFormViewModel model = new CategoryFormViewModel();
        model.EventID = eventId;
        model.CategoryID = category.CategoryID;
        model.CategoryName = category.CategoryName;
        model.DistanceKm = category.DistanceKm;
        model.EntryFee = category.EntryFee;
        model.MinAge = category.MinAge;
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryFormViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var details = new
        {
            CategoryName = model.CategoryName,
            DistanceKm = model.DistanceKm,
            EntryFee = model.EntryFee,
            MinAge = model.MinAge
        };

        ApiResponse response = await CallApi(HttpMethod.Put, "api/categories/" + model.CategoryID, details);
        if (!response.IsSuccess)
        {
            if (response.StatusCode == 401)
                return GoToLogin();

            ModelState.AddModelError("", _api.GetErrorMessage(response));
            return View(model);
        }

        TempData["Success"] = "Category updated.";
        return RedirectToAction("Index", new { eventId = model.EventID });
    }

    // ---------- delete ----------

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id, int eventId)
    {
        ApiResponse response = await CallApi(HttpMethod.Delete, "api/categories/" + id);

        if (response.StatusCode == 401)
            return GoToLogin();

        if (response.IsSuccess)
            TempData["Success"] = "Category deleted.";
        else
            TempData["Error"] = _api.GetErrorMessage(response);   // for example: people are already enrolled

        return RedirectToAction("Index", new { eventId = eventId });
    }
}