using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;

namespace LicentaInAngular.Server.Repositories
{
    public interface IComandaRepository
    {
        Task<ComandaDTO?> GetById(int id);

        // Get all Comenzi (Orders), without Card details
        Task<IEnumerable<ComandaDTO>> GetAllOrders();
        Task CreateComanda(Comanda comanda);  // Create a new Comanda
        Task UpdateComanda(Comanda comanda);  // Update an existing Comanda
        Task DeleteComanda(int id);  // Delete a Comanda by its ID
        Task<IEnumerable<ComandaDTO>> GetComenziByUserId(int userId);  // Get all Comenzi for a User
        Task<Comanda> SubmitComanda(ComandaSubmitDTO comandaDto);
        Task<bool> MarcheazaComandaCaLivrata(int idComanda);
        Task<int> EmitereComanda(int userId, int idAdresa, int? idCard);
        Task<IEnumerable<ComandaCuDetaliiDTO>> GetToateComenzileAleUtilizatorului(int userId);
    }
}
