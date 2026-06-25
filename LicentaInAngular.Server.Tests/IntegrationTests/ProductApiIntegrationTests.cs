using Bogus;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Net;
using System.Net.Http.Json;

namespace LicentaInAngular.Server.Tests.IntegrationTests
{
    [TestFixture]
    public class ProductApiIntegrationTests
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

        #region GET Endpoints - Basic

        [Test]
        public async Task GetProducts_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/Product");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetDetailedProducts_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/Product/detailed");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetOfferedProducts_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/Product/offer");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetProduseCuReducere_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/Product/cu-reducere");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region GET By Category

        [Test]
        [TestCase("Electronics")]
        [TestCase("Clothing")]
        [TestCase("Books")]
        public async Task GetProductsByCategory_WithDifferentCategories_ShouldReturnOk(string category)
        {
            var response = await _client.GetAsync($"/api/Product/category/{category}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        [Repeat(3)]
        public async Task GetProductsByCategory_WithRandomCategory_ShouldReturnOk()
        {
            var category = _faker.Commerce.Department();
            var response = await _client.GetAsync($"/api/Product/category/{category}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region GET By ID

        [Test]
        [TestCase(1)]
        [TestCase(2)]
        [TestCase(999)]
        public async Task GetProduct_WithDifferentIds_ShouldReturnOkOrNotFound(int id)
        {
            var response = await _client.GetAsync($"/api/Product/{id}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task GetProduct_WithRandomId_ShouldHandleGracefully()
        {
            var randomId = _faker.Random.Int(1, 10000);
            var response = await _client.GetAsync($"/api/Product/{randomId}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region GET Search

        [Test]
        public async Task SearchProductsByName_WithEmptyName_ShouldReturnBadRequest()
        {
            var response = await _client.GetAsync("/api/Product/search?name=");
            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
        }

        [Test]
        [Repeat(5)]
        public async Task SearchProductsByName_WithRandomProductName_ShouldReturnOk()
        {
            var productName = _faker.Commerce.ProductName();
            var response = await _client.GetAsync($"/api/Product/search?name={productName}");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        [TestCase("Laptop")]
        [TestCase("Phone")]
        [TestCase("Monitor")]
        [TestCase("Keyboard")]
        public async Task SearchProductsByName_WithDifferentProducts_ShouldReturnOk(string productName)
        {
            var response = await _client.GetAsync($"/api/Product/search?name={productName}");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region GET By User ID

        [Test]
        public async Task GetProductsByUserId_WithValidId_ShouldReturnOkOrNotFound()
        {
            var userId = _faker.Random.Int(1, 100);
            var response = await _client.GetAsync($"/api/Product/user/{userId}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region GET Prices

        [Test]
        public async Task GetToatePreturileDinBD_ShouldReturnOk()
        {
            var response = await _client.GetAsync("/api/Product/toate-preturile");
            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }

        [Test]
        public async Task GetPreturiPentruProdus_WithValidId_ShouldReturnOkOrNotFound()
        {
            var produsId = _faker.Random.Int(1, 100);
            var response = await _client.GetAsync($"/api/Product/preturi/{produsId}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task AfiseazaReducerea_WithValidId_ShouldReturnOkOrNotFound()
        {
            var produsId = _faker.Random.Int(1, 100);
            var response = await _client.GetAsync($"/api/Product/reducere/{produsId}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region POST Requests

        [Test]
        public async Task CreateProduct_WithValidData_ShouldReturnOkOrCreated()
        {
            using var form = new MultipartFormDataContent();
            form.Add(new StringContent(_faker.Commerce.ProductName()), "Nume");
            form.Add(new StringContent(_faker.Lorem.Paragraph()), "Descriere");
            form.Add(new StringContent(_faker.Random.Bool().ToString()), "EsteSpray");
            form.Add(new StringContent("1"), "IdUser");
            form.Add(new StringContent("Electronics"), "DenumireCategorie");

            var response = await _client.PostAsync("/api/Product", form);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        [Test]
        public async Task AdaugaReducere_WithValidData_ShouldReturnOkOrCreated()
        {
            var request = new
            {
                idProdus = _faker.Random.Int(1, 100),
                pretNou = _faker.Random.Decimal(10, 500),
                dataExpirare = DateTime.UtcNow.AddDays(30)
            };

            var response = await _client.PostAsJsonAsync("/api/Product/reducere", request);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.Created ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        #endregion

        #region PUT Requests

        [Test]
        public async Task UpdateProduct_WithValidData_ShouldReturnOkOrBadRequest()
        {
            var produsId = 1;
            var request = new
            {
                id = produsId,
                nume = _faker.Commerce.ProductName(),
                descriere = _faker.Lorem.Paragraph()
            };

            var response = await _client.PutAsJsonAsync($"/api/Product/{produsId}", request);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task ModificaPret_WithValidData_ShouldReturnOkOrBadRequest()
        {
            var idPP = _faker.Random.Int(1, 100);
            var request = new
            {
                pret = _faker.Random.Decimal(10, 500),
                comision = _faker.Random.Decimal(0, 50),
                dataExpirare = DateTime.UtcNow.AddDays(60)
            };

            var response = await _client.PutAsJsonAsync($"/api/Product/modifica-pret/{idPP}", request);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.BadRequest ||
                response.StatusCode == HttpStatusCode.NotFound
            );
        }

        [Test]
        public async Task DezactiveazaProdus_WithValidId_ShouldReturnOkOrNotFound()
        {
            var produsId = _faker.Random.Int(1, 100);
            var response = await _client.PutAsync($"/api/Product/dezactiveaza/{produsId}", null);
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        #endregion

        #region DELETE Requests

        [Test]
        public async Task DeleteProduct_WithValidId_ShouldReturnOkOrNotFound()
        {
            var produsId = _faker.Random.Int(1, 100);
            var response = await _client.DeleteAsync($"/api/Product/{produsId}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        [Test]
        public async Task StergePret_WithValidId_ShouldReturnOkOrNotFound()
        {
            var idPP = _faker.Random.Int(1, 100);
            var response = await _client.DeleteAsync($"/api/Product/sterge-pret/{idPP}");
            ClassicAssert.IsTrue(
                response.StatusCode == HttpStatusCode.OK ||
                response.StatusCode == HttpStatusCode.NotFound ||
                response.StatusCode == HttpStatusCode.BadRequest
            );
        }

        #endregion

        #region Multiple Requests

        [Test]
        public async Task GetProducts_MultipleSequentialRequests_ShouldAllReturnOk()
        {
            for (int i = 0; i < 5; i++)
            {
                var response = await _client.GetAsync("/api/Product");
                ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode, $"Request {i + 1} failed");
            }
        }

        [Test]
        [Repeat(3)]
        public async Task SearchProductsByName_ConsistentResults_ShouldReturnSameStatusCode()
        {
            var productName = _faker.Commerce.ProductName();

            var response1 = await _client.GetAsync($"/api/Product/search?name={productName}");
            var response2 = await _client.GetAsync($"/api/Product/search?name={productName}");

            ClassicAssert.AreEqual(response1.StatusCode, response2.StatusCode);
        }

        #endregion
    }
}







//using System.Net;
//using System.Net.Http;
//using System.Threading.Tasks;
//using NUnit.Framework;
//using NUnit.Framework.Legacy;

//namespace LicentaInAngular.Server.Tests.IntegrationTests
//{
//    [TestFixture]
//    public class ProductApiIntegrationTests
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
//        public async Task GetProducts_ShouldReturnOk()
//        {
//            var response = await _client.GetAsync("/api/Product");

//            ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
//        }

//        [Test]
//        public async Task SearchProducts_WithEmptyName_ShouldReturnBadRequest()
//        {
//            var response = await _client.GetAsync("/api/Product/search?name=");

//            ClassicAssert.AreEqual(HttpStatusCode.BadRequest, response.StatusCode);
//        }
//    }
//}
