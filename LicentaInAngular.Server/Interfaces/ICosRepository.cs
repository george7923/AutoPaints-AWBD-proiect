using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace LicentaInAngular.Server.Repositories
{
    public interface ICosRepository
    {
        Task<Cos?> GetCartByUserId(int userId);

        Task<List<CartProductDTO>> GetProductsByUserId(int userId);

        Task ClearCart(int userId);

        Task<Cos?> GetById(int id);

        Task<IEnumerable<Cos>> GetAllCarts();

        Task CreateCart(Cos cos);

        Task UpdateCart(Cos cos);

        Task DeleteCart(int id);

        Task DeleteOldCarts(DateTime thresholdDate);

        // Metode Response mutate din CosController
        Task<IActionResult> GetCartByUserIdResponse(int userId);

        Task<IActionResult> GetCartByIdResponse(int id);

        Task<IActionResult> CheckOrCreateCartResponse(int userId);

        Task<IActionResult> GetCartDetailsByUserResponse(int userId);

        Task<IActionResult> AddOneSubprodusToCartResponse(SubprodusUpdateDTO request);

        Task<IActionResult> RemoveOneSubprodusFromCartResponse(SubprodusUpdateDTO request);

        Task<IActionResult> RemoveAllSubproduseFromCartResponse(SubprodusUpdateDTO request);

        Task<IActionResult> DeleteCartByUserIdResponse(int userId);

        Task<IActionResult> AddMultipleSubproduseToCartResponse(SubprodusAdaugareDTO dto);

        Task<IActionResult> GetCartContentByUserIdResponse(int userId);
    }
}
