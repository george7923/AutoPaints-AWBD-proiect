using Bogus;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Net;

namespace LicentaInAngular.Server.Tests.IntegrationTests
{
    [TestFixture]
    public class PaginationApiIntegrationTests
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

        #region Basic Pagination Tests

        [Test]
        public async Task GetProductsPaginated_WithDefaultParams_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/Pagination/products");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetProductsPaginated_WithValidPageAndPageSize_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/Pagination/products?page=1&pageSize=10");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(3)]
        [TestCase(5)]
        public async Task GetProductsPaginated_WithDifferentPages_ShouldReturnOkOrNotFound(int page)
        {
            var response = await _client.GetAsync($"/api/Pagination/products?page={page}&pageSize=10");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        [TestCase(5)]
        [TestCase(10)]
        [TestCase(20)]
        [TestCase(50)]
        public async Task GetProductsPaginated_WithDifferentPageSizes_ShouldReturnOkOrBadRequest(int pageSize)
        {
            var response = await _client.GetAsync($"/api/Pagination/products?page=1&pageSize={pageSize}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        [Test]
        public async Task GetProductsPaginated_WithInvalidPage_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Pagination/products?page=0&pageSize=10");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetProductsPaginated_WithInvalidPageSize_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Pagination/products?page=1&pageSize=0");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task GetProductsPaginated_WithPageSizeOver100_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Pagination/products?page=1&pageSize=150");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        [Repeat(3)]
        public async Task GetProductsPaginated_WithRandomValidParams_ShouldReturnOkOrNotFound()
        {
            var page = _faker.Random.Int(1, 10);
            var pageSize = _faker.Random.Int(5, 50);

            var response = await _client.GetAsync($"/api/Pagination/products?page={page}&pageSize={pageSize}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region Search Pagination Tests

        [Test]
        public async Task SearchProductsPaginated_WithValidSearchTerm_ShouldReturnOkOrNotFound()
        {
            var searchTerm = _faker.Commerce.ProductName();
            var response = await _client.GetAsync($"/api/Pagination/products/search?searchTerm={searchTerm}&page=1&pageSize=10");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task SearchProductsPaginated_WithEmptySearchTerm_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Pagination/products/search?searchTerm=&page=1&pageSize=10");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        [Repeat(5)]
        public async Task SearchProductsPaginated_WithRandomTerms_ShouldReturnOkOrNotFound()
        {
            var searchTerm = _faker.Random.Word();
            var page = _faker.Random.Int(1, 5);
            var pageSize = _faker.Random.Int(5, 20);

            var response = await _client.GetAsync($"/api/Pagination/products/search?searchTerm={searchTerm}&page={page}&pageSize={pageSize}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        #endregion

        #region Category Pagination Tests

        [Test]
        [TestCase("Electronics")]
        [TestCase("Clothing")]
        [TestCase("Books")]
        public async Task GetProductsByCategoryPaginated_WithValidCategory_ShouldReturnOkOrNotFound(string category)
        {
            var response = await _client.GetAsync($"/api/Pagination/products/category?category={category}&page=1&pageSize=10");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task GetProductsByCategoryPaginated_WithEmptyCategory_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Pagination/products/category?category=&page=1&pageSize=10");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        [Repeat(3)]
        public async Task GetProductsByCategoryPaginated_WithRandomCategory_ShouldReturnOkOrNotFound()
        {
            var category = _faker.Commerce.Department();
            var response = await _client.GetAsync($"/api/Pagination/products/category?category={category}&page=1&pageSize=10");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region User Products Pagination Tests

        [Test]
        public async Task GetProductsByUserPaginated_WithValidUserId_ShouldReturnOkOrNotFound()
        {
            var userId = _faker.Random.Int(1, 100);
            var response = await _client.GetAsync($"/api/Pagination/products/user?userId={userId}&page=1&pageSize=10");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task GetProductsByUserPaginated_WithInvalidUserId_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Pagination/products/user?userId=0&page=1&pageSize=10");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        [Repeat(3)]
        public async Task GetProductsByUserPaginated_WithRandomUserId_ShouldReturnOkOrNotFound()
        {
            var userId = _faker.Random.Int(1, 100);
            var page = _faker.Random.Int(1, 5);
            var pageSize = _faker.Random.Int(5, 20);

            var response = await _client.GetAsync($"/api/Pagination/products/user?userId={userId}&page={page}&pageSize={pageSize}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region Discount Products Pagination Tests

        [Test]
        public async Task GetProductsWithDiscountPaginated_WithDefaultParams_ShouldReturnOkOrNotFound()
        {
            var response = await _client.GetAsync("/api/Pagination/products/with-discount");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task GetProductsWithDiscountPaginated_WithValidParams_ShouldReturnOkOrNotFound()
        {
            var response = await _client.GetAsync("/api/Pagination/products/with-discount?page=1&pageSize=10");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task GetProductsWithDiscountPaginated_WithPageSizeOver100_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Pagination/products/with-discount?page=1&pageSize=150");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        #endregion

        #region Offered Products Pagination Tests

        [Test]
        public async Task GetOfferedProductsPaginated_WithDefaultParams_ShouldReturnOkOrNotFound()
        {
            var response = await _client.GetAsync("/api/Pagination/products/offered");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task GetOfferedProductsPaginated_WithValidParams_ShouldReturnOkOrNotFound()
        {
            var response = await _client.GetAsync("/api/Pagination/products/offered?page=1&pageSize=20");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        [Repeat(2)]
        public async Task GetOfferedProductsPaginated_WithRandomParams_ShouldReturnOkOrNotFound()
        {
            var page = _faker.Random.Int(1, 5);
            var pageSize = _faker.Random.Int(10, 30);

            var response = await _client.GetAsync($"/api/Pagination/products/offered?page={page}&pageSize={pageSize}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region Edge Cases & Consistency Tests

        [Test]
        public async Task Pagination_WithNegativePage_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Pagination/products?page=-1&pageSize=10");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        public async Task Pagination_WithNegativePageSize_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Pagination/products?page=1&pageSize=-5");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        [Repeat(3)]
        public async Task Pagination_ConsistentResults_SamePage_ShouldReturnSameStatusCode()
        {
            var page = _faker.Random.Int(1, 5);
            var pageSize = _faker.Random.Int(5, 20);

            var response1 = await _client.GetAsync($"/api/Pagination/products?page={page}&pageSize={pageSize}");
            var response2 = await _client.GetAsync($"/api/Pagination/products?page={page}&pageSize={pageSize}");

            ClassicAssert.AreEqual(response1.StatusCode, response2.StatusCode);
        }

        [Test]
        public async Task Pagination_MultipleSequentialRequests_ShouldAllSucceed()
        {
            for (int i = 1; i <= 5; i++)
            {
                var response = await _client.GetAsync($"/api/Pagination/products?page={i}&pageSize=10");
                ClassicAssert.IsTrue(
                    response.StatusCode == HttpStatusCode.OK ||
                    response.StatusCode == HttpStatusCode.NotFound,
                    $"Request for page {i} failed with status {response.StatusCode}"
                );
            }
        }

        #endregion
    }
}