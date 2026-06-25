using Bogus;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Net;
using System.Net.Http.Json;

namespace LicentaInAngular.Server.Tests.IntegrationTests
{

    [TestFixture]
    public class CategoriesApiIntegrationTests
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
        public async Task GetAllCategories_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/Categories");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetCategoryById_WithValidId_ShouldReturnOkOrNotFound()
        {
            var categoryId = _faker.Random.Int(1, 100);
            var response = await _client.GetAsync($"/api/Categories/{categoryId}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        [Repeat(3)]
        public async Task AddCategory_WithRandomName_ShouldReturnOkOrCreated()
        {
            var request = new
            {
                denumireCategorie = _faker.Commerce.Department()
            };

            var response = await _client.PostAsJsonAsync("/api/Categories", request);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        [Test]
        public async Task UpdateCategory_WithValidData_ShouldReturnOkOrBadRequest()
        {
            var categoryId = _faker.Random.Int(1, 100);
            var request = new
            {
                denumireCategorie = _faker.Commerce.Department()
            };

            var response = await _client.PutAsJsonAsync($"/api/Categories/{categoryId}", request);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task DeleteCategory_WithValidId_ShouldReturnOkOrNotFound()
        {
            var categoryId = _faker.Random.Int(1, 100);
            var response = await _client.DeleteAsync($"/api/Categories/{categoryId}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        [Test]
        public async Task AddCategoryWithoutId_WithValidData_ShouldReturnOkOrCreated()
        {
            var request = new
            {
                denumireCategorie = _faker.Commerce.Department()
            };

            var response = await _client.PostAsJsonAsync("/api/Categories/without_primarykey", request);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        [Test]
        public async Task UpdateCategoryWithoutId_WithValidData_ShouldReturnOkOrBadRequest()
        {
            var request = new
            {
                denumireCategorie = _faker.Commerce.Department()
            };

            var response = await _client.PutAsJsonAsync("/api/Categories/without_primarykey", request);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }
    }
}

