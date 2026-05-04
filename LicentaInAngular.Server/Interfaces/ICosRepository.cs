using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.DataLayer.DTO;

namespace LicentaInAngular.Server.Repositories
{
    public interface ICosRepository
    {
        Task<Cos?> GetCartByUserId(int userId);
        Task<List<CartProductDTO>> GetProductsByUserId(int userId);
        Task ClearCart(int userId);

        // Additional methods if needed:
        Task<Cos?> GetById(int id);
        Task<IEnumerable<Cos>> GetAllCarts();
        Task CreateCart(Cos cos);
        Task UpdateCart(Cos cos);
        Task DeleteCart(int id);
        Task DeleteOldCarts(DateTime thresholdDate);
    }
}
