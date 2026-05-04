using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface ICategoryRepository
    {
        Task<IEnumerable<Categorii>> GetAllCategories();
        Task<Categorii> GetCategoryById(int id);
        Task AddCategory(Categorii category);
        Task UpdateCategory(Categorii category);
        Task DeleteCategory(int id);

        // metoda necesară pentru a actualiza / căuta după nume
        Task<Categorii> GetCategoryByName(string denumireCategorie);
    }
}
