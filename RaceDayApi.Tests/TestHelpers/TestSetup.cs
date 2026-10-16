using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;

namespace RaceDayApi.Tests
{// start of namespace
    public static class TestSetup
    {// start of class

        // the secret code organisers need when they register (only used in the tests)
        public const string OrganiserCode = "TEST-ORG-CODE";

        // the password every test user gets
        public const string Password = "Password123";

        // makes a brand new EMPTY database in memory, so tests never affect each other
        public static RaceDayDbContext NewDb()
        {
            DbContextOptionsBuilder<RaceDayDbContext> builder = new DbContextOptionsBuilder<RaceDayDbContext>();
            builder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            return new RaceDayDbContext(builder.Options);
        }

        // a pretend appsettings.json that only contains the organiser code
        public static IConfiguration NewConfig()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>();
            settings.Add("OrganiserCode", OrganiserCode);

            ConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddInMemoryCollection(settings);
            return builder.Build();
        }

        // pretends this user is logged in (puts their ID and role into the controller's session)
        public static void LogIn(ControllerBase controller, User user)
        {
            DefaultHttpContext http = new DefaultHttpContext();
            http.Session = new FakeSession();
            http.Session.SetInt32(SessionKeys.UserId, user.UserID);
            http.Session.SetString(SessionKeys.Role, user.Role);

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = http;
        }

        // pretends nobody is logged in (an empty session)
        public static void NobodyLoggedIn(ControllerBase controller)
        {
            DefaultHttpContext http = new DefaultHttpContext();
            http.Session = new FakeSession();

            controller.ControllerContext = new ControllerContext();
            controller.ControllerContext.HttpContext = http;
        }

        // turns a controller result into its status code number (201, 403 and so on)
        public static int GetStatus(IActionResult result)
        {
            // every result our endpoints return (Ok, NotFound, StatusCode...) has a status code
            IStatusCodeActionResult with_code = (IStatusCodeActionResult)result;
            return with_code.StatusCode.Value;
        }

        // ---------- sample data. These save straight into the test database ----------

        public static User AddUser(RaceDayDbContext db, string name, string email, string role)
        {
            User new_user = new User();
            new_user.FullName = name;
            new_user.Email = email.ToLower();
            new_user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(Password, 4);   // 4 keeps the tests fast
            new_user.Role = role;

            db.Users.Add(new_user);
            db.SaveChanges();
            return new_user;
        }

        public static Event AddEvent(RaceDayDbContext db, int organiser_id, string name = "Joburg City Run", int days_from_now = 30)
        {
            Event new_event = new Event();
            new_event.OrganiserID = organiser_id;
            new_event.EventName = name;
            new_event.Description = "Test event";
            new_event.EventDate = DateTime.Today.AddDays(days_from_now);
            new_event.Location = "Johannesburg";
            new_event.DistanceKm = 10;
            new_event.EventType = "Run";

            db.Events.Add(new_event);
            db.SaveChanges();
            return new_event;
        }

        public static Category AddCategory(RaceDayDbContext db, int event_id, string name = "10km")
        {
            Category new_category = new Category();
            new_category.EventID = event_id;
            new_category.CategoryName = name;
            new_category.DistanceKm = 10;
            new_category.EntryFee = 150;

            db.Categories.Add(new_category);
            db.SaveChanges();
            return new_category;
        }

        public static Registration AddEnrolment(RaceDayDbContext db, int participant_id, int category_id, string race_number = "RD10001")
        {
            Registration new_enrolment = new Registration();
            new_enrolment.ParticipantID = participant_id;
            new_enrolment.CategoryID = category_id;
            new_enrolment.RaceNumber = race_number;

            db.Registrations.Add(new_enrolment);
            db.SaveChanges();
            return new_enrolment;
        }

        // a valid event the tests can send to the API
        public static EventDto ValidEventDto()
        {
            EventDto dto = new EventDto();
            dto.EventName = "Joburg City Run";
            dto.Description = "A fun 10km run through the city.";
            dto.EventDate = DateTime.Today.AddDays(30);
            dto.Location = "Johannesburg";
            dto.DistanceKm = 10;
            dto.EventType = "Run";
            return dto;
        }

    }// end of class
}// end of namespace