using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.IntegrationTests
{
    [TestFixture]
    public class ProductApiIntegrationTests
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
        public async Task GetProducts_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/Product");

            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task SearchProducts_WithEmptyName_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Product/search?name=");

            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }
    }
}
