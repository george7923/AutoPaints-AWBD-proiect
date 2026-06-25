using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Bogus;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.IntegrationTests
{
    [TestFixture]
    public class AccountApiIntegrationTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;
        private Faker _faker = null!;

        [SetUp]
        public void Before()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
            _faker = new Faker();
        }

        [TearDown]
        public void After()
        {
            _client.Dispose();
            _factory.Dispose();
        }

        [Test]
        public async Task Login_WithInvalidCredentials_ShouldNotReturnSuccess()
        {
            var request = new
            {
                username = _faker.Person.UserName,
                password = _faker.Internet.Password()
            };

            var response = await _client.PostAsJsonAsync("/api/Account/login", request);
            ClassicAssert.AreNotEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        [Repeat(5)]
        public async Task Login_WithRandomInvalidCredentials_MultipleTimes()
        {
            var request = new
            {
                username = _faker.Person.UserName,
                password = _faker.Internet.Password()
            };

            var response = await _client.PostAsJsonAsync("/api/Account/login", request);
            ClassicAssert.AreNotEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Login_WithEmptyUsername_ShouldReturnBadRequest()
        {
            var request = new
            {
                username = string.Empty,
                password = _faker.Internet.Password()
            };

            var response = await _client.PostAsJsonAsync("/api/Account/login", request);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Login_WithEmptyPassword_ShouldReturnBadRequest()
        {
            var request = new
            {
                username = _faker.Person.UserName,
                password = string.Empty
            };

            var response = await _client.PostAsJsonAsync("/api/Account/login", request);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Register_WithValidData_ShouldReturnCreated()
        {
            var request = new
            {
                username = _faker.Person.UserName,
                email = _faker.Person.Email,
                password = _faker.Internet.Password(length: 10),
                confirmPassword = _faker.Internet.Password(length: 10)
            };

            var response = await _client.PostAsJsonAsync("/api/Account/register", request);
            ClassicAssert.AreEqual(HttpStatusCode.Created, response.StatusCode);
        }

        [Test]
        public async Task Register_WithInvalidEmail_ShouldReturnBadRequest()
        {
            var request = new
            {
                username = _faker.Person.UserName,
                email = "invalid-email",
                password = _faker.Internet.Password(length: 10),
                confirmPassword = _faker.Internet.Password(length: 10)
            };

            var response = await _client.PostAsJsonAsync("/api/Account/register", request);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Register_WithPasswordMismatch_ShouldReturnBadRequest()
        {
            var password = _faker.Internet.Password(length: 10);

            var request = new
            {
                username = _faker.Person.UserName,
                email = _faker.Person.Email,
                password = password,
                confirmPassword = "different_password"
            };

            var response = await _client.PostAsJsonAsync("/api/Account/register", request);
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}







//using System.Net;
//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Threading.Tasks;
//using NUnit.Framework;
//using NUnit.Framework.Legacy;
//namespace LicentaInAngular.Server.Tests.IntegrationTests
//{
//    [TestFixture]
//    public class AccountApiIntegrationTests
//    {
//        private CustomWebApplicationFactory _factory = null!;
//        private HttpClient _client = null!;
//        [SetUp]
//        public void Before()
//        {
//            _factory = new CustomWebApplicationFactory();
//            _client = _factory.CreateClient();
//        }
//        [TearDown]
//        public void After()
//        {
//            _client.Dispose();
//            _factory.Dispose();
//        }
//        [Test]
//        public async Task Login_WithInvalidCredentials_ShouldNotReturnSuccess()
//        {
//            var request = new
//            {
//                username = "user_inexistent_test",
//                password = "parola_gresita_test"
//            };
//            var response = await _client.PostAsJsonAsync("/api/Account/login", request);
//            ClassicAssert.AreNotEqual(HttpStatusCode.OK, response.StatusCode);
//        }
//    }
//}