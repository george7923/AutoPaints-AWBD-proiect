using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LicentaInAngular.Server.DataLayer.DTO;

namespace LicentaInAngular.Server.Repositories
{
    public class SubprodusRepository : ISubprodusRepository
    {
        private readonly ApplicationDbContext _context;

        public SubprodusRepository(ApplicationDbContext context)
        {
            _context = context;
        }

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

                    // ✅ Get latest price from `Preturi_Produs`
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
                    Pret = pret != null ? pret.Pret : 0, // ✅ Latest price
                    Categorie = cat.DenumireCategorie ?? "Necunoscut", // ✅ Category from `Categorii`
                    EsteSpray = p.EsteSpray,
                    CodCuloare = vop != null ? vop.CodCuloare : "N/A", // ✅ Color Code from `Vopsea`
                    Imagine = img != null ? img.Fisier : new byte[0] // ✅ Image from `Imagini`
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
            var subprodus = await _context.SubProduse.FirstOrDefaultAsync(sp => sp.IdSubprodus == idSubprodus);
            return subprodus != null && subprodus.Valabil;
        }
        public async Task<int> CountSubproduseByProdusId(int idProdus)
        {
            return await _context.SubProduse.CountAsync(sp => sp.IdProdus == idProdus);
        }
        public async Task<int> CountAvailableSubproduseByProdusId(int idProdus)
        {
            return await _context.SubProduse.CountAsync(sp => sp.IdProdus == idProdus && sp.idCos == null && sp.Valabil == true);
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

    }
}
