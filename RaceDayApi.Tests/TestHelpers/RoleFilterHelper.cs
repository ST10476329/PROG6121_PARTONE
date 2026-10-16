using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Routing;
using RaceDayApi.Helpers;

namespace RaceDayApi.Tests
{// start of namespace
    public static class RoleChecker
    {// start of class

        // this number means "the rule let the request through"
        public const int Allowed = 0;

        // finds the [RequireRole] rule written above an endpoint method
        public static RequireRoleAttribute GetRule(Type controller_type, string method_name)
        {
            MethodInfo method = controller_type.GetMethod(method_name);
            object[] rules = method.GetCustomAttributes(typeof(RequireRoleAttribute), true);
            return (RequireRoleAttribute)rules[0];
        }

        // runs the rule as if NOBODY is logged in. Gives back 401, 403 or Allowed
        public static int NotLoggedIn(RequireRoleAttribute rule)
        {
            FakeSession session = new FakeSession();
            return RunRule(rule, session);
        }

        // runs the rule as if someone with this role is logged in. Gives back 401, 403 or Allowed
        public static int LoggedInAs(RequireRoleAttribute rule, string role)
        {
            FakeSession session = new FakeSession();
            session.SetInt32(SessionKeys.UserId, 1);
            session.SetString(SessionKeys.Role, role);
            return RunRule(rule, session);
        }

        private static int RunRule(RequireRoleAttribute rule, FakeSession session)
        {
            DefaultHttpContext http = new DefaultHttpContext();
            http.Session = session;

            ActionContext action_context = new ActionContext(http, new RouteData(), new ActionDescriptor());
            AuthorizationFilterContext context = new AuthorizationFilterContext(action_context, new List<IFilterMetadata>());

            rule.OnAuthorization(context);

            // no result means the rule did not block the request
            if (context.Result == null)
                return Allowed;

            return TestSetup.GetStatus(context.Result);
        }

    }// end of class
}// end of namespace