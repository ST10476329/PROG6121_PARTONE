using RaceDayApi.Controllers;
using RaceDayApi.Helpers;
using Xunit;

namespace RaceDayApi.Tests
{// start of namespace
    public class testing_roleEnforcement
    {// start of class

        // ================= NOBODY LOGGED IN (expect 401) =================

        // nobody logged in cannot view events
        [Fact]
        public void notLoggedIn_viewEvents_unauthorized()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EventsController), "GetAll");

            int results = RoleChecker.NotLoggedIn(rule);
            int expect_results = 401;

            Assert.Equal(expect_results, results);
        }

        // nobody logged in cannot create an event
        [Fact]
        public void notLoggedIn_createEvent_unauthorized()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EventsController), "Create");

            int results = RoleChecker.NotLoggedIn(rule);
            int expect_results = 401;

            Assert.Equal(expect_results, results);
        }

        // nobody logged in cannot delete an event
        [Fact]
        public void notLoggedIn_deleteEvent_unauthorized()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EventsController), "Delete");

            int results = RoleChecker.NotLoggedIn(rule);
            int expect_results = 401;

            Assert.Equal(expect_results, results);
        }

        // nobody logged in cannot enrol
        [Fact]
        public void notLoggedIn_enrol_unauthorized()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EnrolmentsController), "Enrol");

            int results = RoleChecker.NotLoggedIn(rule);
            int expect_results = 401;

            Assert.Equal(expect_results, results);
        }

        // nobody logged in cannot view results
        [Fact]
        public void notLoggedIn_viewResults_unauthorized()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(ResultsController), "GetMine");

            int results = RoleChecker.NotLoggedIn(rule);
            int expect_results = 401;

            Assert.Equal(expect_results, results);
        }

        // ================= A PARTICIPANT USING ORGANISER FUNCTIONS (expect 403) =================

        // a participant cannot create an event
        [Fact]
        public void participant_createEvent_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EventsController), "Create");

            int results = RoleChecker.LoggedInAs(rule, Roles.Participant);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // a participant cannot update an event
        [Fact]
        public void participant_updateEvent_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EventsController), "Update");

            int results = RoleChecker.LoggedInAs(rule, Roles.Participant);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // a participant cannot delete an event
        [Fact]
        public void participant_deleteEvent_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EventsController), "Delete");

            int results = RoleChecker.LoggedInAs(rule, Roles.Participant);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // a participant cannot add a category
        [Fact]
        public void participant_createCategory_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(CategoriesController), "Create");

            int results = RoleChecker.LoggedInAs(rule, Roles.Participant);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // a participant cannot see everyone enrolled in an event
        [Fact]
        public void participant_viewEventEnrolments_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EnrolmentsController), "GetForEvent");

            int results = RoleChecker.LoggedInAs(rule, Roles.Participant);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // a participant cannot capture a result
        [Fact]
        public void participant_captureResult_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(ResultsController), "Create");

            int results = RoleChecker.LoggedInAs(rule, Roles.Participant);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // ================= AN ORGANISER USING PARTICIPANT FUNCTIONS (expect 403) =================

        // an organiser cannot enrol in an event
        [Fact]
        public void organiser_enrol_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EnrolmentsController), "Enrol");

            int results = RoleChecker.LoggedInAs(rule, Roles.Organiser);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // an organiser cannot view "my enrolments"
        [Fact]
        public void organiser_viewMyEnrolments_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EnrolmentsController), "GetMine");

            int results = RoleChecker.LoggedInAs(rule, Roles.Organiser);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // an organiser cannot cancel an enrolment
        [Fact]
        public void organiser_cancelEnrolment_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EnrolmentsController), "Cancel");

            int results = RoleChecker.LoggedInAs(rule, Roles.Organiser);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // an organiser cannot view "my results"
        [Fact]
        public void organiser_viewMyResults_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(ResultsController), "GetMine");

            int results = RoleChecker.LoggedInAs(rule, Roles.Organiser);
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // ================= A MADE-UP ROLE (expect 403) =================

        // someone with a role that does not exist (like Admin) gets nowhere
        [Fact]
        public void unknownRole_createEvent_forbidden()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EventsController), "Create");

            int results = RoleChecker.LoggedInAs(rule, "Admin");
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // ================= THE RIGHT ROLE IS LET THROUGH =================

        // an organiser can create an event
        [Fact]
        public void organiser_createEvent_allowed()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EventsController), "Create");

            int results = RoleChecker.LoggedInAs(rule, Roles.Organiser);
            int expect_results = RoleChecker.Allowed;

            Assert.Equal(expect_results, results);
        }

        // an organiser can add a category
        [Fact]
        public void organiser_createCategory_allowed()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(CategoriesController), "Create");

            int results = RoleChecker.LoggedInAs(rule, Roles.Organiser);
            int expect_results = RoleChecker.Allowed;

            Assert.Equal(expect_results, results);
        }

        // an organiser can see everyone enrolled in an event
        [Fact]
        public void organiser_viewEventEnrolments_allowed()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EnrolmentsController), "GetForEvent");

            int results = RoleChecker.LoggedInAs(rule, Roles.Organiser);
            int expect_results = RoleChecker.Allowed;

            Assert.Equal(expect_results, results);
        }

        // a participant can enrol in an event
        [Fact]
        public void participant_enrol_allowed()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EnrolmentsController), "Enrol");

            int results = RoleChecker.LoggedInAs(rule, Roles.Participant);
            int expect_results = RoleChecker.Allowed;

            Assert.Equal(expect_results, results);
        }

        // a participant can view their results
        [Fact]
        public void participant_viewMyResults_allowed()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(ResultsController), "GetMine");

            int results = RoleChecker.LoggedInAs(rule, Roles.Participant);
            int expect_results = RoleChecker.Allowed;

            Assert.Equal(expect_results, results);
        }

        // both roles can view events
        [Fact]
        public void bothRoles_viewEvents_allowed()
        {
            RequireRoleAttribute rule = RoleChecker.GetRule(typeof(EventsController), "GetAll");

            int organiser_results = RoleChecker.LoggedInAs(rule, Roles.Organiser);
            int participant_results = RoleChecker.LoggedInAs(rule, Roles.Participant);
            int expect_results = RoleChecker.Allowed;

            Assert.Equal(expect_results, organiser_results);
            Assert.Equal(expect_results, participant_results);
        }

    }// end of class
}// end of namespace