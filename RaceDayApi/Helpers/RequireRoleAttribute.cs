using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RaceDayApi.Helpers
{
    public class RequireRoleAttribute : Attribute, IAuthorizationFilter
    {
        private readonly string[] _roles;

        //  if no roles passed in any logged-in user is fine
        public RequireRoleAttribute(params string[] roles)
        {
            _roles = roles;
        }

        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var userId = context.HttpContext.Session.GetInt32(SessionKeys.UserId);
            var role = context.HttpContext.Session.GetString(SessionKeys.Role);

            //  if not logged in they get error 401
            if (userId == null || role == null)
            {
                context.Result = new UnauthorizedObjectResult(new { message = "Please log in first." });
                return;
            }

            // if logged in but the wrong role they get error 403
            if (_roles.Length > 0 && !_roles.Contains(role))
            {
                context.Result = new ObjectResult(new { message = "You do not have permission to do this." })
                {
                    StatusCode = 403
                };
            }
        }
    }
}
