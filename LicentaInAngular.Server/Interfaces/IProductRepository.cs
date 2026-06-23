using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DTOs;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface IProductRepository
    {
        Task<IEnumerable<Produs>> GetAll();

        Task<Produs> GetById(int id);

        Task<IEnumerable<Produs>> SearchByName(string name);

        Task<IEnumerable<SablonProdusDTO>> GetByCategory(string category);

        Task<IEnumerable<SablonProdusDTO>> GetByCategories(List<string> categories);

        Task<IEnumerable<Produs>> GetByUserId(int userId);

        Task CreateProduct(Produs product);

        Task UpdateProduct(Produs product);

        Task DeleteProduct(int id);

        Task<IEnumerable<ProdusDTO>> GetProdusDTOsAsync();

        Task<IEnumerable<ProdusDTO>> GetOfferedProductsAsync();

        // Metode Response mutate din ProductController / ProdusController
        Task<IActionResult> GetProductsResponse();

        Task<IActionResult> GetProductsByCategoryResponse(string category);

        Task<IActionResult> GetProductsByCategoriesResponse(List<string> categories);

        Task<IActionResult> GetProductsByUserIdResponse(int userId);

        Task<IActionResult> GetProductResponse(int id);

        Task<IActionResult> SearchProductsByNameResponse(string name);

        Task<IActionResult> CreateProductResponse(ProductUploadDto productDto);

        Task<IActionResult> UpdateProductResponse(int id, Produs product);

        Task<IActionResult> DeleteProductResponse(int id);

        Task<IActionResult> GetDetailedProductsResponse();

        Task<IActionResult> GetOfferedProductsResponse();

        Task<IActionResult> AdaugaReducereResponse(Reducere_DTO dto);

        Task<IActionResult> AfisareToatePreturileDinBDResponse();

        Task<IActionResult> AfiseazaReducereaResponse(int idProdus);

        Task<IActionResult> GetProduseCuReducereResponse();

        Task<IActionResult> AdminUpdateProdusResponse(int idProdus, AdminUpdateProdus_DTO dto);

        Task<IActionResult> StergePretResponse(int idPP);

        Task<IActionResult> GetPreturiPentruProdusResponse(int idProdus);

        Task<IActionResult> ModificaPretResponse(int idPP, UpdatePret_DTO dto);

        Task<IActionResult> DezactiveazaProdusResponse(int idProdus);
    }
}
