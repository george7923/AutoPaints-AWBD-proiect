using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DTOs;
using LicentaInAngular.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface IProductRepository
    {
        // Get all products
        Task<IEnumerable<Produs>> GetAll();

        // Get product by Id
        Task<Produs> GetById(int id);

        Task<IEnumerable<Produs>> SearchByName(string name);

        // Get products by category
        Task<IEnumerable<SablonProdusDTO>> GetByCategory(string category);

        // Get products by a list of categories
        Task<IEnumerable<SablonProdusDTO>> GetByCategories(List<string> categories);

        // Get products by UserId (Admin)
        Task<IEnumerable<Produs>> GetByUserId(int userId);

        // Create a new product
        Task CreateProduct(Produs product);

        // Update an existing product
        Task UpdateProduct(Produs product);

        // Delete a product by Id
        Task DeleteProduct(int id);
     
        Task<IEnumerable<ProdusDTO>> GetProdusDTOsAsync();
        // IProductRepository.cs
        Task<IEnumerable<ProdusDTO>> GetOfferedProductsAsync();

    }
}
