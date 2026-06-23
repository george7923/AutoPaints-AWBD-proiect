using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.IntegrationTests
{
    [TestFixture]
    public class AccountApiIntegrationTests
    {
        private CustomWebApplicationFactory _factory = null!;
        private HttpClient _client = null!;

        [SetUp]
        public void Before()
        {
            _factory = new CustomWebApplicationFactory();
            _client = _factory.CreateClient();
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
                username = "user_inexistent_test",
                password = "parola_gresita_test"
            };

            var response = await _client.PostAsJsonAsync("/api/Account/login", request);

            ClassicAssert.AreNotEqual(HttpStatusCode.OK, response.StatusCode);
        }
    }
}
