using Microsoft.AspNetCore.Http;
using RaceDayApi.Controllers;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Helpers;
using RaceDayApi.Models;
using Xunit;

namespace RaceDayApi.Tests
{// start of namespace
    public class testing_login
    {// start of class

        private RaceDayDbContext db;
        private AuthController test_auth;
        private User participant;
        private User organiser;

        // runs before every test
        public testing_login()
        {
            db = TestSetup.NewDb();
            participant = TestSetup.AddUser(db, "xaba luyanda", "xaba@gmail.com", Roles.Participant);
            organiser = TestSetup.AddUser(db, "Lebo Dlamini", "lebo@test.com", Roles.Organiser);

            test_auth = new AuthController(db, TestSetup.NewConfig());
            TestSetup.NobodyLoggedIn(test_auth);   // starts with an empty session
        }

        // builds login details to send to the API
        private LoginDto NewLogin(string email, string password)
        {
            LoginDto dto = new LoginDto();
            dto.Email = email;
            dto.Password = password;
            return dto;
        }

        // correct email and password logs the user in
        [Fact]
        public async Task login_correctDetails_success()
        {
            LoginDto details = NewLogin("xaba@gmail.com", TestSetup.Password);

            int results = TestSetup.GetStatus(await test_auth.Login(details));
            int expect_results = 200;

            Assert.Equal(expect_results, results);
        }

        // after login the session must remember the participant's ID and role
        [Fact]
        public async Task login_participant_sessionStoresIdAndRole()
        {
            await test_auth.Login(NewLogin("xaba@gmail.com", TestSetup.Password));

            int? saved_id = test_auth.HttpContext.Session.GetInt32(SessionKeys.UserId);
            string saved_role = test_auth.HttpContext.Session.GetString(SessionKeys.Role);

            Assert.Equal(participant.UserID, saved_id);
            Assert.Equal("Participant", saved_role);
        }

        // after login the session must remember the organiser's ID and role
        [Fact]
        public async Task login_organiser_sessionStoresIdAndRole()
        {
            await test_auth.Login(NewLogin("lebo@test.com", TestSetup.Password));

            int? saved_id = test_auth.HttpContext.Session.GetInt32(SessionKeys.UserId);
            string saved_role = test_auth.HttpContext.Session.GetString(SessionKeys.Role);

            Assert.Equal(organiser.UserID, saved_id);
            Assert.Equal("Organiser", saved_role);
        }

        // a wrong password is rejected
        [Fact]
        public async Task login_wrongPassword_unauthorized()
        {
            LoginDto details = NewLogin("xaba@gmail.com", "WrongPassword1");

            int results = TestSetup.GetStatus(await test_auth.Login(details));
            int expect_results = 401;

            Assert.Equal(expect_results, results);
        }

        // an email that does not exist is rejected
        [Fact]
        public async Task login_unknownEmail_unauthorized()
        {
            LoginDto details = NewLogin("nobody@test.com", TestSetup.Password);

            int results = TestSetup.GetStatus(await test_auth.Login(details));
            int expect_results = 401;

            Assert.Equal(expect_results, results);
        }

        // a failed login must NOT create a session
        [Fact]
        public async Task login_wrongPassword_noSessionCreated()
        {
            await test_auth.Login(NewLogin("xaba@gmail.com", "WrongPassword1"));

            int? saved_id = test_auth.HttpContext.Session.GetInt32(SessionKeys.UserId);

            Assert.Null(saved_id);
        }

        // logging in as someone else replaces the old session (so roles can be switched)
        [Fact]
        public async Task login_secondUser_replacesFirstSession()
        {
            await test_auth.Login(NewLogin("xaba@gmail.com", TestSetup.Password));
            await test_auth.Login(NewLogin("lebo@test.com", TestSetup.Password));

            string saved_role = test_auth.HttpContext.Session.GetString(SessionKeys.Role);

            Assert.Equal("Organiser", saved_role);
        }

        // logging out clears the session
        [Fact]
        public async Task logout_clearsTheSession()
        {
            await test_auth.Login(NewLogin("xaba@gmail.com", TestSetup.Password));

            int results = TestSetup.GetStatus(test_auth.Logout());
            int expect_results = 200;

            Assert.Equal(expect_results, results);
            Assert.Null(test_auth.HttpContext.Session.GetInt32(SessionKeys.UserId));
        }

    }// end of class
}// end of namespace