using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Tests.TestHelpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NUnit.Framework;
using NUnit.Framework.Legacy;

namespace LicentaInAngular.Server.Tests.UnitTests
{
    [TestFixture]
    public class VopseaServiceTests
    {
        private ApplicationDbContext _context = null!;
        private VopseaService _service = null!;

        [SetUp]
        public void SetUp()
        {
            _context = TestDbContextFactory.CreateContext();
            _service = new VopseaService(_context);
        }

        [TearDown]
        public void TearDown()
        {
            _context.Dispose();
        }

        [Test]
        public async Task CreareVopseaSiAdaugaInCosResponse_WhenNewData_ShouldCreateCartCategoryProductPriceMarcaModelVopseaAndSubproduse()
        {
            // Arrange
            var dto = CreateValidDto(
                idUser: 1,
                tipVopsea: "spray",
                cantitate: 3,
                marca: "BMW",
                model: "Seria 3",
                an: 2020
            );

            // Act
            var result = await _service.CreareVopseaSiAdaugaInCosResponse(dto);

            // Assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var cosuriCount = await _context.Cosuri.CountAsync();
            var categoriiCount = await _context.Categorii.CountAsync();
            var produseCount = await _context.Products.CountAsync();
            var preturiCount = await _context.Preturi_Produs.CountAsync();
            var marciCount = await _context.Marci.CountAsync();
            var modeleCount = await _context.Modele.CountAsync();
            var vopseleCount = await _context.Vopsele.CountAsync();
            var subproduseCount = await _context.SubProduse.CountAsync();

            ClassicAssert.AreEqual(1, cosuriCount);
            ClassicAssert.AreEqual(1, categoriiCount);
            ClassicAssert.AreEqual(1, produseCount);
            ClassicAssert.AreEqual(1, preturiCount);
            ClassicAssert.AreEqual(1, marciCount);
            ClassicAssert.AreEqual(1, modeleCount);
            ClassicAssert.AreEqual(1, vopseleCount);
            ClassicAssert.AreEqual(3, subproduseCount);

            var produs = await _context.Products.FirstAsync();
            ClassicAssert.AreEqual("spray auto personalizata", produs.Nume);
            ClassicAssert.IsTrue(produs.Valabil);
            ClassicAssert.IsTrue(produs.EsteSpray);

            var pret = await _context.Preturi_Produs.FirstAsync();
            ClassicAssert.AreEqual(60, pret.Pret);

            var categorie = await _context.Categorii.FirstAsync();
            ClassicAssert.AreEqual("spray vopsea auto preparat dupa cod", categorie.DenumireCategorie);

            var marca = await _context.Marci.FirstAsync();
            ClassicAssert.AreEqual("BMW", marca.NumeMarca);

            var model = await _context.Modele.FirstAsync();
            ClassicAssert.AreEqual("Seria 3", model.NumeModel);
            ClassicAssert.AreEqual(2020, model.An);

            var vopsea = await _context.Vopsele.FirstAsync();
            ClassicAssert.AreEqual("spray", vopsea.TipVopsea);
            ClassicAssert.AreEqual(dto.CodCuloare, vopsea.CodCuloare);
            ClassicAssert.AreEqual(dto.SerieCaroserie, vopsea.SerieCaroserie);

            var okResult = (OkObjectResult)result;
            ClassicAssert.IsNotNull(okResult.Value);

            var subproduseCreateInCos = GetAnonymousProperty<int>(okResult.Value!, "SubproduseCreateInCos");
            ClassicAssert.AreEqual(3, subproduseCreateInCos);
        }

        [Test]
        public async Task CreareVopseaSiAdaugaInCosResponse_WhenTipVopseaIsNotSpray_ShouldCreateNormalPaintWithPrice35()
        {
            // Arrange
            var dto = CreateValidDto(
                idUser: 2,
                tipVopsea: "cutie",
                cantitate: 2,
                marca: "Audi",
                model: "A4",
                an: 2019
            );

            // Act
            var result = await _service.CreareVopseaSiAdaugaInCosResponse(dto);

            // Assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var categorie = await _context.Categorii.FirstAsync();
            var pret = await _context.Preturi_Produs.FirstAsync();
            var subproduseCount = await _context.SubProduse.CountAsync();

            ClassicAssert.AreEqual("vopsea auto preparata dupa codul de culoare al masinii", categorie.DenumireCategorie);
            ClassicAssert.AreEqual(35, pret.Pret);
            ClassicAssert.AreEqual(2, subproduseCount);
        }

        [Test]
        public async Task CreareVopseaSiAdaugaInCosResponse_WhenCartCategoryMarcaAndModelAlreadyExist_ShouldReuseExistingEntities()
        {
            // Arrange
            var existingCart = new Cos
            {
                CodUnic = "existing-cart",
                IdUser = 10,
                DataCreare = DateTime.UtcNow.AddDays(-1)
            };

            var existingCategory = new Categorii
            {
                DenumireCategorie = "spray vopsea auto preparat dupa cod",
                DescriereCategorie = "Categorie existenta"
            };

            var existingMarca = new Marca
            {
                NumeMarca = "Mercedes"
            };

            _context.Cosuri.Add(existingCart);
            _context.Categorii.Add(existingCategory);
            _context.Marci.Add(existingMarca);
            await _context.SaveChangesAsync();

            var existingModel = new Model
            {
                NumeModel = "C Class",
                IdMarca = existingMarca.IdMarca,
                An = 2021
            };

            _context.Modele.Add(existingModel);
            await _context.SaveChangesAsync();

            var dto = CreateValidDto(
                idUser: 10,
                tipVopsea: "spray",
                cantitate: 1,
                marca: "Mercedes",
                model: "C Class",
                an: 2021
            );

            // Act
            var result = await _service.CreareVopseaSiAdaugaInCosResponse(dto);

            // Assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var cosuriCount = await _context.Cosuri.CountAsync();
            var categoriiCount = await _context.Categorii.CountAsync();
            var marciCount = await _context.Marci.CountAsync();
            var modeleCount = await _context.Modele.CountAsync();
            var produseCount = await _context.Products.CountAsync();
            var vopseleCount = await _context.Vopsele.CountAsync();
            var subproduseCount = await _context.SubProduse.CountAsync();

            ClassicAssert.AreEqual(1, cosuriCount);
            ClassicAssert.AreEqual(1, categoriiCount);
            ClassicAssert.AreEqual(1, marciCount);
            ClassicAssert.AreEqual(1, modeleCount);
            ClassicAssert.AreEqual(1, produseCount);
            ClassicAssert.AreEqual(1, vopseleCount);
            ClassicAssert.AreEqual(1, subproduseCount);
        }

        [Test]
        public async Task CreareVopseaSiAdaugaInCosResponse_WhenProductNameAlreadyExists_ShouldCreateProductWithSuffix()
        {
            // Arrange
            var category = new Categorii
            {
                DenumireCategorie = "spray vopsea auto preparat dupa cod",
                DescriereCategorie = "Categorie existenta"
            };

            _context.Categorii.Add(category);
            await _context.SaveChangesAsync();

            _context.Products.Add(new Produs
            {
                Nume = "spray auto personalizata",
                Descriere = "Produs deja existent",
                EsteSpray = true,
                Valabil = true,
                IdCategorie = category.IdCategorie
            });

            await _context.SaveChangesAsync();

            var dto = CreateValidDto(
                idUser: 20,
                tipVopsea: "spray",
                cantitate: 1,
                marca: "Ford",
                model: "Focus",
                an: 2018
            );

            // Act
            var result = await _service.CreareVopseaSiAdaugaInCosResponse(dto);

            // Assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var createdProduct = await _context.Products
                .OrderByDescending(p => p.IdProdus)
                .FirstAsync();

            ClassicAssert.AreEqual("spray auto personalizata (2)", createdProduct.Nume);
        }

        [Test]
        public async Task CreareVopseaSiAdaugaInCosResponse_WhenMultipleProductNamesAlreadyExist_ShouldCreateNextAvailableSuffix()
        {
            // Arrange
            var category = new Categorii
            {
                DenumireCategorie = "spray vopsea auto preparat dupa cod",
                DescriereCategorie = "Categorie existenta"
            };

            _context.Categorii.Add(category);
            await _context.SaveChangesAsync();

            _context.Products.AddRange(
                new Produs
                {
                    Nume = "spray auto personalizata",
                    Descriere = "Produs existent 1",
                    EsteSpray = true,
                    Valabil = true,
                    IdCategorie = category.IdCategorie
                },
                new Produs
                {
                    Nume = "spray auto personalizata (2)",
                    Descriere = "Produs existent 2",
                    EsteSpray = true,
                    Valabil = true,
                    IdCategorie = category.IdCategorie
                }
            );

            await _context.SaveChangesAsync();

            var dto = CreateValidDto(
                idUser: 21,
                tipVopsea: "spray",
                cantitate: 1,
                marca: "Opel",
                model: "Astra",
                an: 2016
            );

            // Act
            var result = await _service.CreareVopseaSiAdaugaInCosResponse(dto);

            // Assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var createdProduct = await _context.Products
                .OrderByDescending(p => p.IdProdus)
                .FirstAsync();

            ClassicAssert.AreEqual("spray auto personalizata (3)", createdProduct.Nume);
        }

        [Test]
        public async Task CreareVopseaSiAdaugaInCosResponse_WhenCantitateIsZero_ShouldNotCreateSubproduse()
        {
            // Arrange
            var dto = CreateValidDto(
                idUser: 30,
                tipVopsea: "spray",
                cantitate: 0,
                marca: "Dacia",
                model: "Logan",
                an: 2022
            );

            // Act
            var result = await _service.CreareVopseaSiAdaugaInCosResponse(dto);

            // Assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var subproduseCount = await _context.SubProduse.CountAsync();
            ClassicAssert.AreEqual(0, subproduseCount);

            var okResult = (OkObjectResult)result;
            ClassicAssert.IsNotNull(okResult.Value);

            var subproduseCreateInCos = GetAnonymousProperty<int>(okResult.Value!, "SubproduseCreateInCos");
            ClassicAssert.AreEqual(0, subproduseCreateInCos);
        }

        [Test]
        public async Task CreareVopseaSiAdaugaInCosResponse_WhenMarcaMasiniiIsNull_ShouldReturnStatusCode500()
        {
            // Arrange
            var dto = CreateValidDto(
                idUser: 40,
                tipVopsea: "spray",
                cantitate: 1,
                marca: null,
                model: "Golf",
                an: 2017
            );

            // Act
            var result = await _service.CreareVopseaSiAdaugaInCosResponse(dto);

            // Assert
            ClassicAssert.IsInstanceOf<ObjectResult>(result);

            var objectResult = (ObjectResult)result;
            ClassicAssert.AreEqual(500, objectResult.StatusCode);

            ClassicAssert.IsNotNull(objectResult.Value);

            var message = GetAnonymousProperty<string>(objectResult.Value!, "message");
            ClassicAssert.AreEqual("Eroare la crearea vopselei/produsului + adăugare în coș.", message);
        }

        [Test]
        public async Task CreareVopseaSiAdaugaInCosResponse_WhenModelIsNull_ShouldReturnStatusCode500()
        {
            // Arrange
            var dto = CreateValidDto(
                idUser: 41,
                tipVopsea: "spray",
                cantitate: 1,
                marca: "Volkswagen",
                model: null,
                an: 2017
            );

            // Act
            var result = await _service.CreareVopseaSiAdaugaInCosResponse(dto);

            // Assert
            ClassicAssert.IsInstanceOf<ObjectResult>(result);

            var objectResult = (ObjectResult)result;
            ClassicAssert.AreEqual(500, objectResult.StatusCode);
        }

        [Test]
        public async Task CreareVopseaSiAdaugaInCosResponse_ShouldReturnOkObjectWithIdCosIdVopseaAndProductDto()
        {
            // Arrange
            var dto = CreateValidDto(
                idUser: 50,
                tipVopsea: "spray",
                cantitate: 2,
                marca: "Toyota",
                model: "Corolla",
                an: 2021
            );

            // Act
            var result = await _service.CreareVopseaSiAdaugaInCosResponse(dto);

            // Assert
            ClassicAssert.IsInstanceOf<OkObjectResult>(result);

            var okResult = (OkObjectResult)result;
            ClassicAssert.IsNotNull(okResult.Value);

            var message = GetAnonymousProperty<string>(okResult.Value!, "message");
            var idVopsea = GetAnonymousProperty<int>(okResult.Value!, "IdVopsea");
            var idCos = GetAnonymousProperty<int>(okResult.Value!, "IdCos");
            var subproduseCreateInCos = GetAnonymousProperty<int>(okResult.Value!, "SubproduseCreateInCos");
            var produsDto = GetAnonymousProperty<SablonProdusDTO>(okResult.Value!, "Produs");

            ClassicAssert.AreEqual("Vopsea + Produs + Subproduse create și adăugate în coș.", message);
            ClassicAssert.IsTrue(idVopsea > 0);
            ClassicAssert.IsTrue(idCos > 0);
            ClassicAssert.AreEqual(2, subproduseCreateInCos);
            ClassicAssert.IsNotNull(produsDto);
            ClassicAssert.AreEqual("spray auto personalizata", produsDto.Nume);
            ClassicAssert.AreEqual(60, produsDto.Pret);
            ClassicAssert.AreEqual("spray vopsea auto preparat dupa cod", produsDto.Categorie);
        }

        private static CreateVopseaSiAdaugaInCosDto CreateValidDto(
            int idUser,
            string tipVopsea,
            int cantitate,
            string? marca,
            string? model,
            int an)
        {
            return new CreateVopseaSiAdaugaInCosDto
            {
                IdUser = idUser,
                TipVopsea = tipVopsea,
                MarcaMasinii = marca!,
                Model = model!,
                An = an,
                CodCuloare = "A123",
                SerieCaroserie = "WBA123456789",
                DetaliiSuplimentare = "Test detalii vopsea",
                Cantitate = cantitate
            };
        }

        private static T GetAnonymousProperty<T>(object source, string propertyName)
        {
            var property = source.GetType().GetProperty(propertyName);

            if (property == null)
            {
                ClassicAssert.Fail($"Proprietatea '{propertyName}' nu există în obiectul returnat.");
                throw new InvalidOperationException($"Proprietatea '{propertyName}' nu există în obiectul returnat.");
            }

            var value = property.GetValue(source);

            if (value == null)
            {
                ClassicAssert.Fail($"Proprietatea '{propertyName}' este null.");
                throw new InvalidOperationException($"Proprietatea '{propertyName}' este null.");
            }

            if (value is not T)
            {
                ClassicAssert.Fail($"Proprietatea '{propertyName}' nu este de tipul așteptat: {typeof(T).Name}.");
                throw new InvalidOperationException($"Proprietatea '{propertyName}' nu este de tipul așteptat: {typeof(T).Name}.");
            }

            return (T)value;
        }
    }
}