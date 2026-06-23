using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.IntegrationTests
{
    [TestFixture]
    public class GptApiIntegrationTests
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
        public async Task Chat_WithEmptyPrompt_ShouldReturnBadRequest()
        {
            var request = new
            {
                prompt = ""
            };

            var response = await _client.PostAsJsonAsync("/api/GPT/chat", request);

            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Chat_WithValidPrompt_ShouldReturnOk()
        {
            var request = new
            {
                prompt = "Salut"
            };

            var response = await _client.PostAsJsonAsync("/api/GPT/chat", request);

            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var body = await response.Content.ReadAsStringAsync();

            StringAssert.Contains("Raspuns fake pentru test", body);
        }
    }
}
