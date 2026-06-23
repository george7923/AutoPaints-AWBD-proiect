using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.DTO;

namespace LicentaInAngular.Server.Repositories
{
    public class SubprodusService : ISubprodusRepository
    {
        private readonly ApplicationDbContext _context;

        public SubprodusService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // METODE EXISTENTE DIN ISubprodusRepository
        // =========================

        // Get all the products for a specific Cart (Cos)
        public async Task<IEnumerable<ProdusCosDTO>> GetProdusByCosId(int idCos)
        {
            /*
            var products = await (
                from sp in _context.SubProduse
                join p in _context.Products on sp.IdProdus equals p.IdProdus
                join cat in _context.Categorii on p.IdCategorie equals cat.IdCategorie
                join vop in _context.Vopsele on p.IdProdus equals vop.IdProdus into vopGroup
                from vop in vopGroup.DefaultIfEmpty()
                join img in _context.Imagini on p.IdProdus equals img.IdProdus into imgGroup
                from img in imgGroup.DefaultIfEmpty()

                    // Get latest price from `Preturi_Produs`
                join pretGroup in _context.Preturi_Produs
                    .GroupBy(pp => pp.IdProdus)
                    .(g => g.OrderByDescending(pp => pp.DataInceput).FirstOrDefault()) // Latest price
                on p.IdProdus equals pretGroup.IdProdus into pretJoin
                from pret in pretJoin.DefaultIfEmpty()

                where sp.idCos == idCos

                 new ProdusCosDTO
                {
                    IdSubprodus = sp.IdSubprodus,
                    IdProdus = p.IdProdus,
                    Nume = p.Nume ?? "N/A",
                    Pret = pret != null ? pret.Pret : 0, // Latest price
                    Categorie = cat.DenumireCategorie ?? "Necunoscut",
                    EsteSpray = p.EsteSpray,
                    CodCuloare = vop != null ? vop.CodCuloare : "N/A",
                    Imagine = img != null ? img.Fisier : new byte[0]
                }).ToListAsync();

            return products;
            */
            return null;
        }

        // Get all Subprodus records
        public async Task<IEnumerable<Subprodus>> GetAllSubproduse()
        {
            return await _context.SubProduse.ToListAsync();
        }

        // Get Subprodus by IdSubprodus
        public async Task<Subprodus> GetSubprodusById(int idSubprodus)
        {
            return await _context.SubProduse
                .FirstOrDefaultAsync(sp => sp.IdSubprodus == idSubprodus);
        }

        // Create a new Subprodus
        public async Task CreateSubprodus(Subprodus subprodus)
        {
            await _context.SubProduse.AddAsync(subprodus);
            await _context.SaveChangesAsync();
        }

        // Delete Subprodus by IdSubprodus
        public async Task DeleteSubprodus(int idSubprodus)
        {
            var subprodus = await _context.SubProduse.FindAsync(idSubprodus);

            if (subprodus != null)
            {
                _context.SubProduse.Remove(subprodus);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteSubprodus(int idCos, int idProdus)
        {
            var subprodus = await _context.SubProduse
                .FirstOrDefaultAsync(sp => sp.idCos == idCos && sp.IdProdus == idProdus);

            if (subprodus != null)
            {
                _context.SubProduse.Remove(subprodus);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteSubproduseByProdusId(int idProdus)
        {
            var subproduse = await _context.SubProduse
                .Where(sp => sp.IdProdus == idProdus)
                .ToListAsync();

            if (subproduse.Any())
            {
                _context.SubProduse.RemoveRange(subproduse);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteSubproduseByCosId(int idCos)
        {
            var subproduse = await _context.SubProduse
                .Where(sp => sp.idCos == idCos)
                .ToListAsync();

            if (subproduse.Any())
            {
                _context.SubProduse.RemoveRange(subproduse);
                await _context.SaveChangesAsync();
            }
        }

        // Get all Subprodus records for a specific Produs
        public async Task<IEnumerable<Subprodus>> GetSubprodusByProdusId(int idProdus)
        {
            return await _context.SubProduse
                .Where(sp => sp.IdProdus == idProdus)
                .Include(sp => sp.Cos)
                .ToListAsync();
        }

        public async Task<bool> IsSubprodusValabil(int idSubprodus)
        {
            var subprodus = await _context.SubProduse
                .FirstOrDefaultAsync(sp => sp.IdSubprodus == idSubprodus);

            return subprodus != null && subprodus.Valabil;
        }

        public async Task<int> CountSubproduseByProdusId(int idProdus)
        {
            return await _context.SubProduse
                .CountAsync(sp => sp.IdProdus == idProdus);
        }

        public async Task<int> CountAvailableSubproduseByProdusId(int idProdus)
        {
            return await _context.SubProduse
                .CountAsync(sp => sp.IdProdus == idProdus && sp.idCos == null && sp.Valabil == true);
        }

        public async Task<List<Subprodus>> GetAvailableSubproduse(int idProdus, int cantitate)
        {
            return await _context.SubProduse
                .Where(sp => sp.IdProdus == idProdus && sp.idCos == null)
                .Take(cantitate)
                .ToListAsync();
        }

        public async Task UpdateSubproduse(List<Subprodus> subproduse)
        {
            _context.SubProduse.UpdateRange(subproduse);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ProdusCosDTO_2>> GetProdusByCos_JoinLaToateProdusele(int idCos)
        {
            // Obtinem preturile cele mai recente separat, ca sa putem face join valid
            var preturiRecente = _context.Preturi_Produs
                .GroupBy(pp => pp.IdProdus)
                .Select(g => new
                {
                    IdProdus = g.Key,
                    Pret = g.OrderByDescending(x => x.DataInceput).FirstOrDefault().Pret
                });

            var products = await (
                from sp in _context.SubProduse
                join p in _context.Products on sp.IdProdus equals p.IdProdus
                join cat in _context.Categorii on p.IdCategorie equals cat.IdCategorie
                join vop in _context.Vopsele on p.IdProdus equals vop.IdProdus into vopGroup
                from vop in vopGroup.DefaultIfEmpty()
                join img in _context.Imagini on p.IdProdus equals img.IdProdus into imgGroup
                from img in imgGroup.DefaultIfEmpty()
                join pret in preturiRecente on p.IdProdus equals pret.IdProdus into pretJoin
                from pret in pretJoin.DefaultIfEmpty()

                where sp.idCos == idCos
                group new { sp, p, pret, cat, vop, img } by new
                {
                    p.IdProdus,
                    p.Nume,
                    Pret = pret != null ? pret.Pret : 0,
                    Categorie = cat.DenumireCategorie ?? "Necunoscut",
                    p.EsteSpray,
                    CodCuloare = vop != null ? vop.CodCuloare : "N/A",
                    Imagine = img != null ? img.Fisier : new byte[0]
                } into g
                select new ProdusCosDTO_2
                {
                    IdProdus = g.Key.IdProdus,
                    Nume = g.Key.Nume,
                    Pret = g.Key.Pret,
                    Categorie = g.Key.Categorie,
                    EsteSpray = g.Key.EsteSpray,
                    CodCuloare = g.Key.CodCuloare,
                    Imagine = g.Key.Imagine,
                    Cantitatea = g.Count()
                }).ToListAsync();

            return products;
        }

        // =========================
        // LOGICA MUTATA DIN SubprodusController
        // =========================

        public async Task<IActionResult> GetAllSubproduseResponse()
        {
            var subproduse = await GetAllSubproduse();
            return new OkObjectResult(subproduse);
        }

        public async Task<IActionResult> GetSubprodusByIdResponse(int idSubprodus)
        {
            var subprodus = await GetSubprodusById(idSubprodus);

            if (subprodus == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(subprodus);
        }

        public async Task<IActionResult> CreateSubprodusResponse(Subprodus subprodus, ModelStateDictionary modelState)
        {
            if (!modelState.IsValid)
            {
                var errors = modelState
                    .Where(ms => ms.Value.Errors.Count > 0)
                    .SelectMany(ms => ms.Value.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return new BadRequestObjectResult(new
                {
                    message = "Model invalid",
                    details = errors
                });
            }

            subprodus.Produs = null;
            subprodus.Cos = null;

            try
            {
                await CreateSubprodus(subprodus);

                return new CreatedAtActionResult(
                    "GetSubprodusById",
                    "Subprodus",
                    new { idSubprodus = subprodus.IdSubprodus },
                    subprodus
                );
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(new
                {
                    message = ex.Message
                });
            }
        }

        public async Task<IActionResult> DeleteSubprodusResponse(int idSubprodus)
        {
            await DeleteSubprodus(idSubprodus);
            return new NoContentResult();
        }

        public async Task<IActionResult> DeleteSubprodusByCosAndProdusResponse(int idCos, int idProdus)
        {
            try
            {
                await DeleteSubprodus(idCos, idProdus);
                return new NoContentResult();
            }
            catch (Exception)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Subprodus not found for the given idCos and IdProdus."
                });
            }
        }

        public async Task<IActionResult> DeleteSubproduseByProdusIdResponse(int idProdus)
        {
            try
            {
                await DeleteSubproduseByProdusId(idProdus);
                return new NoContentResult();
            }
            catch (Exception)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Error deleting Subproduse for the given IdProdus."
                });
            }
        }

        public async Task<IActionResult> DeleteSubproduseByCosIdResponse(int idCos)
        {
            try
            {
                await DeleteSubproduseByCosId(idCos);
                return new NoContentResult();
            }
            catch (Exception)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Error deleting Subproduse for the given idCos."
                });
            }
        }

        public async Task<IActionResult> GetSubprodusByProdusIdResponse(int idProdus)
        {
            var subproduse = await GetSubprodusByProdusId(idProdus);
            return new OkObjectResult(subproduse);
        }

        public async Task<IActionResult> CountSubproduseByProdusIdResponse(int idProdus)
        {
            int count = await CountSubproduseByProdusId(idProdus);

            return new OkObjectResult(new
            {
                Count = count
            });
        }

        public async Task<IActionResult> IsSubprodusValabilResponse(int idSubprodus)
        {
            bool isValabil = await IsSubprodusValabil(idSubprodus);

            return new OkObjectResult(new
            {
                IsValabil = isValabil
            });
        }

        public async Task<IActionResult> CountAvailableSubproduseByProdusIdResponse(int idProdus)
        {
            int count = await CountAvailableSubproduseByProdusId(idProdus);

            return new OkObjectResult(new
            {
                Count = count
            });
        }

        public async Task<IActionResult> CountAvailableSubproduseResponse(int produsId)
        {
            var count = await _context.SubProduse
                .Where(s => s.IdProdus == produsId && s.idCos == null && s.Valabil == true)
                .CountAsync();

            return new OkObjectResult(count);
        }

        public async Task<IActionResult> GetProdusByCos_JoinLaToateProduseleResponse(int idCos)
        {
            var produse = await GetProdusByCos_JoinLaToateProdusele(idCos);

            if (produse == null || !produse.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products found for this Cos."
                });
            }

            return new OkObjectResult(produse);
        }
    }
}