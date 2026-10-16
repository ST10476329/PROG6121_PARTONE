using RaceDayApi.Controllers;
using RaceDayApi.Data;
using RaceDayApi.Dtos;
using RaceDayApi.Models;
using Xunit;

namespace RaceDayApi.Tests
{// start of namespace
    public class testing_registration
    {// start of class

        private RaceDayDbContext db;
        private AuthController test_auth;

        // runs before EVERY test, so every test gets a fresh empty database
        public testing_registration()
        {
            db = TestSetup.NewDb();
            test_auth = new AuthController(db, TestSetup.NewConfig());
        }

        // builds registration details to send to the API
        private RegisterDto NewUser(string email, string role, string code)
        {
            RegisterDto dto = new RegisterDto();
            dto.FullName = "xaba luyanda";
            dto.Email = email;
            dto.Password = TestSetup.Password;
            dto.Role = role;
            dto.OrganiserCode = code;
            return dto;
        }

        // a participant registering with good details is created
        [Fact]
        public async Task register_participant_success()
        {// start of method

            // temp details to test
            RegisterDto new_user = NewUser("xaba@gmail.com", "Participant", null);

            // get the results and the expected results
            int results = TestSetup.GetStatus(await test_auth.Register(new_user));
            int expect_results = 201;

            // use assert to compare
            Assert.Equal(expect_results, results);

        }// end of method

        // the new user must really be saved with the right details
        [Fact]
        public async Task register_user_isSavedInDatabase()
        {
            RegisterDto new_user = NewUser("xaba@gmail.com", "Participant", null);

            await test_auth.Register(new_user);
            User saved = db.Users.Single();

            Assert.Equal("xaba@gmail.com", saved.Email);
            Assert.Equal("Participant", saved.Role);
            Assert.Equal("xaba luyanda", saved.FullName);
        }

        // the password must be hashed, never stored as typed
        [Fact]
        public async Task register_password_isHashedNotPlainText()
        {
            RegisterDto new_user = NewUser("xaba@gmail.com", "Participant", null);

            await test_auth.Register(new_user);
            User saved = db.Users.Single();

            Assert.NotEqual(TestSetup.Password, saved.PasswordHash);
            Assert.True(BCrypt.Net.BCrypt.Verify(TestSetup.Password, saved.PasswordHash));
        }

        // an organiser with the correct secret code is created
        [Fact]
        public async Task register_organiser_correctCode_success()
        {
            RegisterDto new_user = NewUser("lebo@test.com", "Organiser", TestSetup.OrganiserCode);

            int results = TestSetup.GetStatus(await test_auth.Register(new_user));
            int expect_results = 201;

            Assert.Equal(expect_results, results);
        }

        // an organiser with the wrong code is refused and nothing is saved
        [Fact]
        public async Task register_organiser_wrongCode_forbidden()
        {
            RegisterDto new_user = NewUser("lebo@test.com", "Organiser", "WRONG-CODE");

            int results = TestSetup.GetStatus(await test_auth.Register(new_user));
            int expect_results = 403;

            Assert.Equal(expect_results, results);
            Assert.Empty(db.Users);
        }

        // an organiser who sends no code at all is refused
        [Fact]
        public async Task register_organiser_noCode_forbidden()
        {
            RegisterDto new_user = NewUser("lebo@test.com", "Organiser", null);

            int results = TestSetup.GetStatus(await test_auth.Register(new_user));
            int expect_results = 403;

            Assert.Equal(expect_results, results);
        }

        // registering with an email that already exists is a conflict
        [Fact]
        public async Task register_duplicateEmail_conflict()
        {
            TestSetup.AddUser(db, "Xaba Luyanda", "xaba@gmail.com", "Participant");

            // same email but with capital letters, which must still count as the same email
            RegisterDto new_user = NewUser("xaba@gmail.com", "Participant", null);

            int results = TestSetup.GetStatus(await test_auth.Register(new_user));
            int expect_results = 409;

            Assert.Equal(expect_results, results);
        }

        // a role that does not exist (like Admin) is a bad request
        [Fact]
        public async Task register_invalidRole_badRequest()
        {
            RegisterDto new_user = NewUser("xaba@gmail.com", "Admin", null);

            int results = TestSetup.GetStatus(await test_auth.Register(new_user));
            int expect_results = 400;

            Assert.Equal(expect_results, results);
        }

    }// end of class
}// end of namespace