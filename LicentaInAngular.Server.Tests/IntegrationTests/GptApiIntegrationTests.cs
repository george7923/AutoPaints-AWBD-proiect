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
    public class GptApiIntegrationTests
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

        [Test]
        [Repeat(5)]
        public async Task Chat_WithRandomPrompt_ShouldReturnOk()
        {
            var request = new
            {
                prompt = _faker.Lorem.Sentence()
            };
            var response = await _client.PostAsJsonAsync("/api/GPT/chat", request);
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Chat_WithLongPrompt_ShouldReturnOk()
        {
            var request = new
            {
                prompt = _faker.Lorem.Paragraphs(3)
            };
            var response = await _client.PostAsJsonAsync("/api/GPT/chat", request);
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task Chat_WithSpecialCharacters_ShouldHandleGracefully()
        {
            var request = new
            {
                prompt = "!@#$%^&*() 你好 مرحبا"
            };
            var response = await _client.PostAsJsonAsync("/api/GPT/chat", request);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        [Test]
        public async Task Chat_WithMultipleRequests_ShouldReturnOkForAll()
        {
            for (int i = 0; i < 5; i++)
            {
                var request = new
                {
                    prompt = _faker.Lorem.Sentence()
                };
                var response = await _client.PostAsJsonAsync("/api/GPT/chat", request);
                ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            }
        }

        [Test]
        [TestCase("Hello world")]
        [TestCase("Salut lume")]
        [TestCase("Hola mundo")]
        [TestCase("Bonjour monde")]
        public async Task Chat_WithDifferentLanguages_ShouldReturnOk(string prompt)
        {
            var request = new { prompt };
            var response = await _client.PostAsJsonAsync("/api/GPT/chat", request);
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
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
//    public class GptApiIntegrationTests
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
//        public async Task Chat_WithEmptyPrompt_ShouldReturnBadRequest()
//        {
//            var request = new
//            {
//                prompt = ""
//            };

//            var response = await _client.PostAsJsonAsync("/api/GPT/chat", request);

//            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
//        }

//        [Test]
//        public async Task Chat_WithValidPrompt_ShouldReturnOk()
//        {
//            var request = new
//            {
//                prompt = "Salut"
//            };

//            var response = await _client.PostAsJsonAsync("/api/GPT/chat", request);

//            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);

//            var body = await response.Content.ReadAsStringAsync();

//            StringAssert.Contains("Raspuns fake pentru test", body);
//        }
//    }
//}
