using System.Collections.Generic;
using System.Threading.Tasks;
using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;

namespace LicentaInAngular.Server.Repositories
{
    public interface IComandaRepository
    {
        Task<ComandaDTO?> GetById(int id);

        Task<IEnumerable<ComandaDTO>> GetAllOrders();

        Task CreateComanda(Comanda comanda);

        Task UpdateComanda(Comanda comanda);

        Task DeleteComanda(int id);

        Task<IEnumerable<ComandaDTO>> GetComenziByUserId(int userId);

        Task<Comanda> SubmitComanda(ComandaSubmitDTO comandaDto);

        Task<bool> MarcheazaComandaCaLivrata(int idComanda);

        Task<int> EmitereComanda(int userId, int idAdresa, int? idCard);

        Task<IEnumerable<ComandaCuDetaliiDTO>> GetToateComenzileAleUtilizatorului(int userId);

        // Metode Response mutate din ComandaController
        Task<IActionResult> GetComandaByIdResponse(int id);

        Task<IActionResult> GetAllComenziResponse();

        Task<IActionResult> GetComenziByUserIdResponse(int userId);

        Task<IActionResult> SubmitComandaWithPaymentResponse(ComandaPaymentDTO dto);

        Task<IActionResult> UpdateComandaResponse(int id, Comanda comanda);

        Task<IActionResult> DeleteComandaResponse(int id);

        Task<IActionResult> EmitereComandaResponse(ComandaEmitereDTO dto, bool isModelValid);

        Task<IActionResult> MarcheazaCaLivrataSauNelivrataResponse(int id, LivrareDto dto);

        Task<IActionResult> GetToateComenzileAleUtilizatoruluiResponse(int userId);

        Task<IActionResult> EmitereComandaCashResponse(ComandaEmitereDTO dto, bool isModelValid);

        Task<IActionResult> GetComenziScurtByUserResponse(int userId);

        Task<IActionResult> GenerareBonFiscalResponse(int idComanda);
    }
}
