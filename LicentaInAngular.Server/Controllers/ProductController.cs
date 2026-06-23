using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.DTOs;
using LicentaInAngular.Server.DataLayer.DTO;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ProductService _productService;

        public ProductController(ProductService productService)
        {
            _productService = productService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            return await _productService.GetProductsResponse();
        }

        [HttpGet("category/{category}")]
        public async Task<IActionResult> GetProductsByCategory(string category)
        {
            return await _productService.GetProductsByCategoryResponse(category);
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetProductsByCategories([FromQuery] List<string> categories)
        {
            return await _productService.GetProductsByCategoriesResponse(categories);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetProductsByUserId(int userId)
        {
            return await _productService.GetProductsByUserIdResponse(userId);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            return await _productService.GetProductResponse(id);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchProductsByName([FromQuery] string name)
        {
            return await _productService.SearchProductsByNameResponse(name);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateProduct([FromForm] ProductUploadDto productDto)
        {
            return await _productService.CreateProductResponse(productDto);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Produs product)
        {
            return await _productService.UpdateProductResponse(id, product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            return await _productService.DeleteProductResponse(id);
        }

        [HttpGet("detailed")]
        public async Task<IActionResult> GetDetailedProducts()
        {
            return await _productService.GetDetailedProductsResponse();
        }

        [HttpGet("offer")]
        public async Task<IActionResult> GetOfferedProducts()
        {
            return await _productService.GetOfferedProductsResponse();
        }

        [HttpPost("reducere")]
        public async Task<IActionResult> AdaugaReducere([FromBody] Reducere_DTO dto)
        {
            return await _productService.AdaugaReducereResponse(dto);
        }

        [HttpGet("toate-preturile")]
        public async Task<IActionResult> AfisareToatePreturileDinBD()
        {
            return await _productService.AfisareToatePreturileDinBDResponse();
        }

        [HttpGet("reducere/{idProdus}")]
        public async Task<IActionResult> AfiseazaReducerea(int idProdus)
        {
            return await _productService.AfiseazaReducereaResponse(idProdus);
        }

        [HttpGet("cu-reducere")]
        public async Task<IActionResult> GetProduseCuReducere()
        {
            return await _productService.GetProduseCuReducereResponse();
        }

        [HttpPut("admin-update/{idProdus}")]
        public async Task<IActionResult> AdminUpdateProdus(int idProdus, [FromBody] AdminUpdateProdus_DTO dto)
        {
            return await _productService.AdminUpdateProdusResponse(idProdus, dto);
        }

        [HttpDelete("sterge-pret/{idPP}")]
        public async Task<IActionResult> StergePret(int idPP)
        {
            return await _productService.StergePretResponse(idPP);
        }

        [HttpGet("preturi/{idProdus}")]
        public async Task<IActionResult> GetPreturiPentruProdus(int idProdus)
        {
            return await _productService.GetPreturiPentruProdusResponse(idProdus);
        }

        [HttpPut("modifica-pret/{idPP}")]
        public async Task<IActionResult> ModificaPret(int idPP, [FromBody] UpdatePret_DTO dto)
        {
            return await _productService.ModificaPretResponse(idPP, dto);
        }

        [HttpPut("dezactiveaza/{idProdus}")]
        public async Task<IActionResult> DezactiveazaProdus(int idProdus)
        {
            return await _productService.DezactiveazaProdusResponse(idProdus);
        }
    }

    public class Reducere_DTO
    {
        public int IdProdus { get; set; }
        public decimal PretNou { get; set; }
        public DateTime? DataExpirare { get; set; }
    }

    public class AdminUpdateProdus_DTO
    {
        public int? IdProdus { get; set; }
        public string? Nume { get; set; }
        public string? Descriere { get; set; }
        public bool? EsteSpray { get; set; }
        public bool? Valabil { get; set; }
        public string? DenumireCategorie { get; set; }
        public int? IdUser { get; set; }
        public string? ImagineBase64 { get; set; }
        public int? Cantitate { get; set; }
    }

    public class UpdatePret_DTO
    {
        public decimal Pret { get; set; }
        public decimal? Comision { get; set; }
        public DateTime? DataExpirare { get; set; }
    }
}