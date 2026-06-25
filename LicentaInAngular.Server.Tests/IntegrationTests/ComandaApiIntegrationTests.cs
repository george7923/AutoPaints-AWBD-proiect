using Bogus;
using NUnit.Framework;
using NUnit.Framework.Legacy;
using System.Net;

namespace LicentaInAngular.Server.Tests.IntegrationTests;

[TestFixture]
public class ComandaApiIntegrationTests
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
    public async Task GetComandaById_WithValidId_ShouldReturnOkOrNotFound()
    {
        var comandaId = _faker.Random.Int(1, 100);
        var response = await _client.GetAsync($"/api/comanda/{comandaId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task GetAllComenzi_ShouldReturnOk()
    {
        var response = await _client.GetAsync("/api/comanda");
        ClassicAssert.AreEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Test]
    public async Task GetComenziByUserId_WithValidUserId_ShouldReturnOkOrNotFound()
    {
        var userId = _faker.Random.Int(1, 100);
        var response = await _client.GetAsync($"/api/comanda/user/{userId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task GetToateComenzileAleUtilizatorului_WithValidUserId_ShouldReturnOkOrNotFound()
    {
        var userId = _faker.Random.Int(1, 100);
        var response = await _client.GetAsync($"/api/comanda/afisare/{userId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task GetComenziScurtByUser_WithValidUserId_ShouldReturnOkOrNotFound()
    {
        var userId = _faker.Random.Int(1, 100);
        var response = await _client.GetAsync($"/api/comanda/scurt/user/{userId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task GenerareBonFiscal_WithValidComandaId_ShouldReturnOkOrNotFound()
    {
        var comandaId = _faker.Random.Int(1, 100);
        var response = await _client.GetAsync($"/api/comanda/generare-pdf/{comandaId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.BadRequest
        );
    }

    [Test]
    [Repeat(2)]
    public async Task SubmitComandaWithPayment_WithValidData_ShouldReturnOkOrBadRequest()
    {
        var request = new
        {
            idCos = _faker.Random.Int(1, 100),
            idUser = _faker.Random.Int(1, 100),
            sumaTotal = _faker.Random.Decimal(50, 5000),
            metodaPlatforma = _faker.Random.Word()
        };

        var response = await _client.PostAsJsonAsync("/api/comanda/submit-with-payment", request);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task UpdateComanda_WithValidData_ShouldReturnOkOrBadRequest()
    {
        var comandaId = _faker.Random.Int(1, 100);
        var request = new
        {
            sumaTotal = _faker.Random.Decimal(50, 5000),
            dataComanda = DateTime.UtcNow
        };

        var response = await _client.PutAsJsonAsync($"/api/comanda/{comandaId}", request);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task DeleteComanda_WithValidId_ShouldReturnOkOrNotFound()
    {
        var comandaId = _faker.Random.Int(1, 100);
        var response = await _client.DeleteAsync($"/api/comanda/{comandaId}");
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.NotFound ||
            response.StatusCode == HttpStatusCode.BadRequest
        );
    }

    [Test]
    [Repeat(2)]
    public async Task EmitereComanda_WithValidData_ShouldReturnOkOrBadRequest()
    {
        var request = new
        {
            idCos = _faker.Random.Int(1, 100),
            numeClient = _faker.Person.FullName,
            emailClient = _faker.Person.Email,
            telefonClient = _faker.Phone.PhoneNumber(),
            adresaDelivery = _faker.Address.FullAddress()
        };

        var response = await _client.PostAsJsonAsync("/api/comanda/emitere", request);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    [Repeat(2)]
    public async Task EmitereComandaCash_WithValidData_ShouldReturnOkOrBadRequest()
    {
        var request = new
        {
            idCos = _faker.Random.Int(1, 100),
            numeClient = _faker.Person.FullName,
            emailClient = _faker.Person.Email,
            telefonClient = _faker.Phone.PhoneNumber(),
            adresaDelivery = _faker.Address.FullAddress()
        };

        var response = await _client.PostAsJsonAsync("/api/comanda/emitere-cash", request);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.Created ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task MarcheazaCaLivrata_WithValidData_ShouldReturnOkOrBadRequest()
    {
        var comandaId = _faker.Random.Int(1, 100);
        var request = new
        {
            livrata = _faker.Random.Bool()
        };

        var response = await _client.PutAsJsonAsync($"/api/comanda/livrare/{comandaId}", request);
        ClassicAssert.IsTrue(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.BadRequest ||
            response.StatusCode == HttpStatusCode.NotFound
        );
    }

    [Test]
    public async Task MultipleOrderRequests_ConsistentResults()
    {
        var userId = _faker.Random.Int(1, 100);

        var response1 = await _client.GetAsync($"/api/comanda/user/{userId}");
        var response2 = await _client.GetAsync($"/api/comanda/user/{userId}");

        ClassicAssert.AreEqual(response1.StatusCode, response2.StatusCode);
    }
}

