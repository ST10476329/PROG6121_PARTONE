using Microsoft.AspNetCore.Mvc;
using RaceDayApi.Controllers;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;
using Xunit;

namespace RaceDayApi.Tests
{// start of namespace
    public class testing_eventManagement
    {// start of class

        private RaceDayDbContext db;
        private User organiser;
        private User other_organiser;
        private User participant;
        private EventsController test_events;   // this controller is logged in as the organiser

        // runs before every test
        public testing_eventManagement()
        {
            db = TestSetup.NewDb();
            organiser = TestSetup.AddUser(db, "Lebo Dlamini", "lebo@test.com", Roles.Organiser);
            other_organiser = TestSetup.AddUser(db, "Sipho Nkosi", "sipho@test.com", Roles.Organiser);
            participant = TestSetup.AddUser(db, "Thabo Mokoena", "thabo@test.com", Roles.Participant);

            test_events = new EventsController(db);
            TestSetup.LogIn(test_events, organiser);
        }

        // ---------- creating events ----------

        // an organiser can create an event
        [Fact]
        public async Task organiser_createsEvent_success()
        {
            EventDto new_event = TestSetup.ValidEventDto();

            int results = TestSetup.GetStatus(await test_events.Create(new_event));
            int expect_results = 201;

            Assert.Equal(expect_results, results);
        }

        // the new event is saved and belongs to the logged-in organiser
        [Fact]
        public async Task organiser_createsEvent_isOwnedByThatOrganiser()
        {
            await test_events.Create(TestSetup.ValidEventDto());

            Event saved = db.Events.Single();

            Assert.Equal(organiser.UserID, saved.OrganiserID);
            Assert.Equal("Joburg City Run", saved.EventName);
            Assert.Equal("Run", saved.EventType);
        }

        // an event type other than Run, Walk or Cycle is a bad request
        [Fact]
        public async Task create_invalidEventType_badRequest()
        {
            EventDto new_event = TestSetup.ValidEventDto();
            new_event.EventType = "Swim";

            int results = TestSetup.GetStatus(await test_events.Create(new_event));
            int expect_results = 400;

            Assert.Equal(expect_results, results);
        }

        // an event date in the past is a bad request
        [Fact]
        public async Task create_dateInThePast_badRequest()
        {
            EventDto new_event = TestSetup.ValidEventDto();
            new_event.EventDate = DateTime.Today.AddDays(-5);

            int results = TestSetup.GetStatus(await test_events.Create(new_event));
            int expect_results = 400;

            Assert.Equal(expect_results, results);
        }

        // ---------- updating events ----------

        // an organiser can update their own event
        [Fact]
        public async Task organiser_updatesOwnEvent_success()
        {
            Event my_event = TestSetup.AddEvent(db, organiser.UserID);
            EventDto changes = TestSetup.ValidEventDto();
            changes.Location = "Pretoria";

            int results = TestSetup.GetStatus(await test_events.Update(my_event.EventID, changes));
            int expect_results = 200;

            Assert.Equal(expect_results, results);
            Assert.Equal("Pretoria", db.Events.Single().Location);
        }

        // an organiser cannot update another organiser's event
        [Fact]
        public async Task update_otherOrganisersEvent_forbidden()
        {
            Event their_event = TestSetup.AddEvent(db, other_organiser.UserID);
            EventDto changes = TestSetup.ValidEventDto();
            changes.Location = "Pretoria";

            int results = TestSetup.GetStatus(await test_events.Update(their_event.EventID, changes));
            int expect_results = 403;

            Assert.Equal(expect_results, results);
            Assert.Equal("Johannesburg", db.Events.Single().Location);   // nothing changed
        }

        // updating an event that does not exist
        [Fact]
        public async Task update_missingEvent_notFound()
        {
            EventDto changes = TestSetup.ValidEventDto();

            int results = TestSetup.GetStatus(await test_events.Update(9999, changes));
            int expect_results = 404;

            Assert.Equal(expect_results, results);
        }

        // ---------- deleting events ----------

        // an organiser can delete their own event
        [Fact]
        public async Task organiser_deletesOwnEvent_success()
        {
            Event my_event = TestSetup.AddEvent(db, organiser.UserID);

            int results = TestSetup.GetStatus(await test_events.Delete(my_event.EventID));
            int expect_results = 204;

            Assert.Equal(expect_results, results);
            Assert.Empty(db.Events);
        }

        // an organiser cannot delete another organiser's event
        [Fact]
        public async Task delete_otherOrganisersEvent_forbidden()
        {
            Event their_event = TestSetup.AddEvent(db, other_organiser.UserID);

            int results = TestSetup.GetStatus(await test_events.Delete(their_event.EventID));
            int expect_results = 403;

            Assert.Equal(expect_results, results);
            Assert.Single(db.Events);   // the event is still there
        }

        // deleting an event that does not exist
        [Fact]
        public async Task delete_missingEvent_notFound()
        {
            int results = TestSetup.GetStatus(await test_events.Delete(9999));
            int expect_results = 404;

            Assert.Equal(expect_results, results);
        }

        // ---------- viewing events ----------

        // viewing all events returns every event
        [Fact]
        public async Task getAll_returnsAllEvents()
        {
            TestSetup.AddEvent(db, organiser.UserID, "Run One");
            TestSetup.AddEvent(db, other_organiser.UserID, "Run Two");

            OkObjectResult ok = (OkObjectResult)await test_events.GetAll();
            IEnumerable<EventResponseDto> all_events = (IEnumerable<EventResponseDto>)ok.Value;

            int results = all_events.Count();
            int expect_results = 2;

            Assert.Equal(expect_results, results);
        }

        // a participant can view events too
        [Fact]
        public async Task participant_viewsEvents_success()
        {
            TestSetup.AddEvent(db, organiser.UserID);

            EventsController participant_events = new EventsController(db);
            TestSetup.LogIn(participant_events, participant);

            int results = TestSetup.GetStatus(await participant_events.GetAll());
            int expect_results = 200;

            Assert.Equal(expect_results, results);
        }

        // viewing one event that exists
        [Fact]
        public async Task getById_existingEvent_success()
        {
            Event my_event = TestSetup.AddEvent(db, organiser.UserID);

            int results = TestSetup.GetStatus(await test_events.GetById(my_event.EventID));
            int expect_results = 200;

            Assert.Equal(expect_results, results);
        }

        // viewing one event that does not exist
        [Fact]
        public async Task getById_missingEvent_notFound()
        {
            int results = TestSetup.GetStatus(await test_events.GetById(9999));
            int expect_results = 404;

            Assert.Equal(expect_results, results);
        }

    }// end of class
}// end of namespace