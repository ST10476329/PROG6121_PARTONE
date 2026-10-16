using Microsoft.AspNetCore.Mvc;
using RaceDayApi.Controllers;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;
using Xunit;

namespace RaceDayApi.Tests
{// start of namespace
    public class testing_enrolments
    {// start of class

        private RaceDayDbContext db;
        private User organiser;
        private User other_organiser;
        private User participant;
        private User other_participant;
        private Event test_event;
        private Category test_category;
        private EnrolmentsController test_enrol;        // logged in as the participant
        private EnrolmentsController organiser_enrol;   // logged in as the organiser

        // runs before every test
        public testing_enrolments()
        {
            db = TestSetup.NewDb();
            organiser = TestSetup.AddUser(db, "Lebo Dlamini", "lebo@test.com", Roles.Organiser);
            other_organiser = TestSetup.AddUser(db, "Sipho Nkosi", "sipho@test.com", Roles.Organiser);
            participant = TestSetup.AddUser(db, "Thabo Mokoena", "thabo@test.com", Roles.Participant);
            other_participant = TestSetup.AddUser(db, "Naledi Khumalo", "naledi@test.com", Roles.Participant);

            test_event = TestSetup.AddEvent(db, organiser.UserID);
            test_category = TestSetup.AddCategory(db, test_event.EventID);

            test_enrol = new EnrolmentsController(db);
            TestSetup.LogIn(test_enrol, participant);

            organiser_enrol = new EnrolmentsController(db);
            TestSetup.LogIn(organiser_enrol, organiser);
        }

        // builds the choice of category to send to the API
        private EnrolmentDto Choice(int category_id)
        {
            EnrolmentDto dto = new EnrolmentDto();
            dto.CategoryID = category_id;
            return dto;
        }

        // ---------- enrolling ----------

        // a participant can enrol in an event under a category
        [Fact]
        public async Task participant_enrols_success()
        {
            EnrolmentDto choice = Choice(test_category.CategoryID);

            int results = TestSetup.GetStatus(await test_enrol.Enrol(test_event.EventID, choice));
            int expect_results = 201;

            Assert.Equal(expect_results, results);
        }

        // the enrolment must link the participant, the category and the event
        [Fact]
        public async Task enrolment_isRecordedCorrectly()
        {
            await test_enrol.Enrol(test_event.EventID, Choice(test_category.CategoryID));

            Registration saved = db.Registrations.Single();
            Category saved_category = db.Categories.Single(c => c.CategoryID == saved.CategoryID);

            Assert.Equal(participant.UserID, saved.ParticipantID);       // who enrolled
            Assert.Equal(test_category.CategoryID, saved.CategoryID);    // which category
            Assert.Equal(test_event.EventID, saved_category.EventID);    // which event (through the category)
            Assert.StartsWith("RD", saved.RaceNumber);                   // a race number was made
        }

        // two participants must never get the same race number
        [Fact]
        public async Task enrolment_raceNumbers_areUnique()
        {
            EnrolmentsController second_enrol = new EnrolmentsController(db);
            TestSetup.LogIn(second_enrol, other_participant);

            await test_enrol.Enrol(test_event.EventID, Choice(test_category.CategoryID));
            await second_enrol.Enrol(test_event.EventID, Choice(test_category.CategoryID));

            int results = db.Registrations.Select(r => r.RaceNumber).Distinct().Count();
            int expect_results = 2;

            Assert.Equal(expect_results, results);
        }

        // enrolling in the same event twice is a conflict, even with a different category
        [Fact]
        public async Task enrol_twiceInSameEvent_conflict()
        {
            Category second_category = TestSetup.AddCategory(db, test_event.EventID, "5km");
            await test_enrol.Enrol(test_event.EventID, Choice(test_category.CategoryID));

            int results = TestSetup.GetStatus(await test_enrol.Enrol(test_event.EventID, Choice(second_category.CategoryID)));
            int expect_results = 409;

            Assert.Equal(expect_results, results);
            Assert.Single(db.Registrations);
        }

        // a category from a different event is a bad request
        [Fact]
        public async Task enrol_categoryFromDifferentEvent_badRequest()
        {
            Event other_event = TestSetup.AddEvent(db, organiser.UserID, "Other Run");
            Category other_category = TestSetup.AddCategory(db, other_event.EventID);

            int results = TestSetup.GetStatus(await test_enrol.Enrol(test_event.EventID, Choice(other_category.CategoryID)));
            int expect_results = 400;

            Assert.Equal(expect_results, results);
        }

        // a category that does not exist
        [Fact]
        public async Task enrol_missingCategory_notFound()
        {
            int results = TestSetup.GetStatus(await test_enrol.Enrol(test_event.EventID, Choice(9999)));
            int expect_results = 404;

            Assert.Equal(expect_results, results);
        }

        // an event that does not exist
        [Fact]
        public async Task enrol_missingEvent_notFound()
        {
            int results = TestSetup.GetStatus(await test_enrol.Enrol(9999, Choice(test_category.CategoryID)));
            int expect_results = 404;

            Assert.Equal(expect_results, results);
        }

        // an event that already happened
        [Fact]
        public async Task enrol_eventInThePast_badRequest()
        {
            Event old_event = TestSetup.AddEvent(db, organiser.UserID, "Old Run", -5);
            Category old_category = TestSetup.AddCategory(db, old_event.EventID);

            int results = TestSetup.GetStatus(await test_enrol.Enrol(old_event.EventID, Choice(old_category.CategoryID)));
            int expect_results = 400;

            Assert.Equal(expect_results, results);
        }

        // ---------- viewing enrolments ----------

        // a participant sees only their own enrolments
        [Fact]
        public async Task participant_viewsOnlyOwnEnrolments()
        {
            TestSetup.AddEnrolment(db, participant.UserID, test_category.CategoryID, "RD10001");
            TestSetup.AddEnrolment(db, other_participant.UserID, test_category.CategoryID, "RD10002");

            OkObjectResult ok = (OkObjectResult)await test_enrol.GetMine();
            List<EnrolmentResponseDto> my_enrolments = ((IEnumerable<EnrolmentResponseDto>)ok.Value).ToList();

            Assert.Single(my_enrolments);
            Assert.Equal(participant.UserID, my_enrolments[0].ParticipantID);
        }

        // an organiser sees everyone enrolled in their own event
        [Fact]
        public async Task organiser_viewsEnrolmentsOfOwnEvent_success()
        {
            TestSetup.AddEnrolment(db, participant.UserID, test_category.CategoryID, "RD10001");
            TestSetup.AddEnrolment(db, other_participant.UserID, test_category.CategoryID, "RD10002");

            OkObjectResult ok = (OkObjectResult)await organiser_enrol.GetForEvent(test_event.EventID);
            IEnumerable<EnrolmentResponseDto> everyone = (IEnumerable<EnrolmentResponseDto>)ok.Value;

            int results = everyone.Count();
            int expect_results = 2;

            Assert.Equal(expect_results, results);
        }

        // an organiser cannot see the enrolments of another organiser's event
        [Fact]
        public async Task organiser_viewsEnrolmentsOfOtherOrganisersEvent_forbidden()
        {
            EnrolmentsController other_organiser_enrol = new EnrolmentsController(db);
            TestSetup.LogIn(other_organiser_enrol, other_organiser);

            int results = TestSetup.GetStatus(await other_organiser_enrol.GetForEvent(test_event.EventID));
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // viewing the enrolments of an event that does not exist
        [Fact]
        public async Task organiser_viewsEnrolments_missingEvent_notFound()
        {
            int results = TestSetup.GetStatus(await organiser_enrol.GetForEvent(9999));
            int expect_results = 404;

            Assert.Equal(expect_results, results);
        }

        // ---------- cancelling ----------

        // a participant can cancel their own enrolment
        [Fact]
        public async Task participant_cancelsOwnEnrolment_success()
        {
            Registration mine = TestSetup.AddEnrolment(db, participant.UserID, test_category.CategoryID);

            int results = TestSetup.GetStatus(await test_enrol.Cancel(mine.RegistrationID));
            int expect_results = 204;

            Assert.Equal(expect_results, results);
            Assert.Empty(db.Registrations);
        }

        // a participant cannot cancel someone else's enrolment
        [Fact]
        public async Task participant_cancelsSomeoneElsesEnrolment_forbidden()
        {
            Registration theirs = TestSetup.AddEnrolment(db, other_participant.UserID, test_category.CategoryID);

            int results = TestSetup.GetStatus(await test_enrol.Cancel(theirs.RegistrationID));
            int expect_results = 403;

            Assert.Equal(expect_results, results);
            Assert.Single(db.Registrations);
        }

        // cancelling an enrolment that does not exist
        [Fact]
        public async Task cancel_missingEnrolment_notFound()
        {
            int results = TestSetup.GetStatus(await test_enrol.Cancel(9999));
            int expect_results = 404;

            Assert.Equal(expect_results, results);
        }

    }// end of class
}// end of namespace