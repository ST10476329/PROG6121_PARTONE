using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayWeb.Helpers;

public class RequireLoginAttribute : ActionFilterAttribute
{
    private readonly string _role;

    public RequireLoginAttribute(string role = null)
    {
        _role = role;
    }

    // runs before the page opens
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        string userRole = context.HttpContext.Session.GetString(SessionKeys.Role);

        // not logged in -> go to the login page
        if (userRole == null)
        {
            context.Result = new RedirectToActionResult("Login", "Account", null);
            return;
        }

        // logged in, but the wrong role -> access denied page
        if (_role != null && userRole != _role)
        {
            context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
        }
    }
}