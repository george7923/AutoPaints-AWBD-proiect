

using Bogus;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Net;

namespace LicentaInAngular.Server.Tests.IntegrationTests;

[TestFixture]
public class CosApiIntegrationTests
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
    public async Task GetCartByUserId_WithValidUserId_ShouldReturnOkOrNotFound()
    {
        var userId = _faker.Random.Int(1, 100);
        var response = await _client.GetAsync($"/api/Cos/user/{userId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task GetCartById_WithValidId_ShouldReturnOkOrNotFound()
    {
        var cartId = _faker.Random.Int(1, 100);
        var response = await _client.GetAsync($"/api/Cos/{cartId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    [Repeat(3)]
    public async Task CheckOrCreateCart_WithRandomUserId_ShouldReturnOkOrCreated()
    {
        var userId = _faker.Random.Int(1, 100);
        var response = await _client.PostAsync($"/api/Cos/check-or-create/{userId}", null);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.BadRequest
        );
    }

    [Test]
    public async Task GetCartDetailsByUser_WithValidUserId_ShouldReturnOkOrNotFound()
    {
        var userId = _faker.Random.Int(1, 100);
        var response = await _client.GetAsync($"/api/Cos/cart-details/{userId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task GetCartContentByUserId_WithValidUserId_ShouldReturnOkOrNotFound()
    {
        var userId = _faker.Random.Int(1, 100);
        var response = await _client.GetAsync($"/api/Cos/cart-content/{userId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task AddOneSubproduct_WithValidData_ShouldReturnOkOrBadRequest()
    {
        var request = new
        {
            idCos = _faker.Random.Int(1, 100),
            idSubprodus = _faker.Random.Int(1, 100),
            cantitate = _faker.Random.Int(1, 10)
        };

        var response = await _client.PutAsJsonAsync("/api/Cos/add-one", request);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task RemoveOneSubproduct_WithValidData_ShouldReturnOkOrBadRequest()
    {
        var request = new
        {
            idCos = _faker.Random.Int(1, 100),
            idSubprodus = _faker.Random.Int(1, 100),
            cantitate = 1
        };

        var response = await _client.PutAsJsonAsync("/api/Cos/remove-one", request);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task RemoveAllSubproducts_WithValidData_ShouldReturnOkOrBadRequest()
    {
        var request = new
        {
            idCos = _faker.Random.Int(1, 100),
            idSubprodus = _faker.Random.Int(1, 100)
        };

        var response = await _client.PutAsJsonAsync("/api/Cos/remove-all", request);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task DeleteCartByUserId_WithValidUserId_ShouldReturnOkOrNotFound()
    {
        var userId = _faker.Random.Int(1, 100);
        var response = await _client.DeleteAsync($"/api/Cos/user/{userId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.BadRequest
        );
    }

    [Test]
    public async Task AddMultipleSubproducts_WithValidData_ShouldReturnOkOrBadRequest()
    {
        var request = new
        {
            idCos = _faker.Random.Int(1, 100),
            subproduseIds = new[] { _faker.Random.Int(1, 100), _faker.Random.Int(1, 100) }
        };

        var response = await _client.PostAsJsonAsync("/api/Cos/addSubproduseToCart", request);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }
}



