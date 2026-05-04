using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.DTOs;
using LicentaInAngular.Server.DataLayer.DTO;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;

    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Produs>> GetAll()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Produs> GetById(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<IEnumerable<Produs>> SearchByName(string name)
    {
        return await _context.Products
            .Where(p => p.Nume.Contains(name)) 
            .ToListAsync();
    }

    public async Task<IEnumerable<SablonProdusDTO>> GetByCategory(string category)
    {
        var categoryLower = category.ToLower();

        var products = await (
            from p in _context.Products
            join cat in _context.Categorii on p.IdCategorie equals cat.IdCategorie
            join vop in _context.Vopsele on p.IdProdus equals vop.IdProdus into vopGroup
            from vop in vopGroup.DefaultIfEmpty()
            join img in _context.Imagini on p.IdProdus equals img.IdProdus into imgGroup
            from img in imgGroup.DefaultIfEmpty()
            join pretGroup in _context.Preturi_Produs
                .GroupBy(pp => pp.IdProdus)
                .Select(g => g.OrderByDescending(pp => pp.DataInceput).FirstOrDefault()) 
            on p.IdProdus equals pretGroup.IdProdus into pretJoin
            from pret in pretJoin.DefaultIfEmpty()

            where cat.DenumireCategorie.ToLower() == categoryLower

            select new SablonProdusDTO
            {
                IdProdus = p.IdProdus,
                Nume = p.Nume,
                Descriere = p.Descriere,
                EsteSpray = p.EsteSpray,
                Pret = pret != null ? pret.Pret : 0, 
                Valabil = p.Valabil,
                Categorie = cat.DenumireCategorie,
                CodCuloare = vop != null ? vop.CodCuloare : "N/A",
                Imagine = img != null ? img.Fisier : new byte[0], 
                IdUser = p.IdUser,
                User = p.User
            }).ToListAsync();

        return products;
    }


    public async Task<IEnumerable<SablonProdusDTO>> GetByCategories(List<string> categories)
    {
        var categoriesLower = categories.Select(c => c.ToLower()).ToList();

        var products = await (
            from p in _context.Products
            join cat in _context.Categorii on p.IdCategorie equals cat.IdCategorie
            join vop in _context.Vopsele on p.IdProdus equals vop.IdProdus into vopGroup
            from vop in vopGroup.DefaultIfEmpty()
            join img in _context.Imagini on p.IdProdus equals img.IdProdus into imgGroup
            from img in imgGroup.DefaultIfEmpty()


            join pretGroup in _context.Preturi_Produs
                .GroupBy(pp => pp.IdProdus) // Group prices by product
                .Select(g => g.OrderByDescending(pp => pp.DataInceput).FirstOrDefault()) // Take latest price
            on p.IdProdus equals pretGroup.IdProdus into pretJoin
            from pret in pretJoin.DefaultIfEmpty()

            where categoriesLower.Contains(cat.DenumireCategorie.ToLower())

            select new SablonProdusDTO
            {
                IdProdus = p.IdProdus,
                Nume = p.Nume,
                Descriere = p.Descriere,
                EsteSpray = p.EsteSpray,
                Pret = pret != null ? pret.Pret : 0,
                Valabil = p.Valabil,
                Categorie = cat.DenumireCategorie,
                CodCuloare = vop != null ? vop.CodCuloare : "N/A",
                Imagine = img != null ? img.Fisier : new byte[0], 
                IdUser = p.IdUser,
                User = p.User
            }).ToListAsync();

        return products;
    }



    public async Task<IEnumerable<Produs>> GetByUserId(int userId)
    {
        return await _context.Products
            .Where(p => p.IdUser == userId)
            .ToListAsync();
    }

    public async Task CreateProduct(Produs product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProduct(Produs product)
    {
        _context.Entry(product).State = EntityState.Modified;  
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProduct(int id)
    {
        var product = await _context.Products.FindAsync(id);
        if (product != null)
        {
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }
    }

    /*
    public async Task<IEnumerable<ProdusDTO>> GetProdusDTOsAsync()
    {
        var query = from produs in _context.Products
                    join user in _context.Users on produs.IdUser equals user.IdUser into pu
                    from user in pu.DefaultIfEmpty()
                    join persoana in _context.Persoane on user.IdPersoana equals persoana.IdPersoana into up
                    from persoana in up.DefaultIfEmpty()
                    select new ProdusDTO
                    {
                        ProdusId = produs.IdProdus,
                        Nume = produs.Nume.Trim(),
                        Descriere = produs.Descriere.Trim(),
                        EsteSprayText = produs.EsteSpray ? "DA" : "NU",
                        Imagine = produs.Imagine,
                        Pret = produs.Pret,
                        Valabil = produs.Valabil,
                        Categorie = produs.Categorie,
                        Vendor = (user != null && user.Username.ToLower() == "admin")
                                    ? "SC ROMCAST EXPERT SRL"
                                    : (persoana != null ? $"{persoana.Nume.Trim()} {persoana.Prenume.Trim()}" : "Necunoscut")
                    };

        return await query.ToListAsync();
    }
    */
    // LEFT JOIN pe Users/persoana la DefaultIfEmpty()
    public async Task<IEnumerable<ProdusDTO>> GetProdusDTOsAsync()
    {
        var lastPrices = await _context.Preturi_Produs
            .GroupBy(x => x.IdProdus)
            .Select(g => new {
                IdProdus = g.Key,
                LastPrice = g.OrderByDescending(x => x.DataInceput)
                             .Select(x => x.Pret)
                             .FirstOrDefault()
            })
            .ToListAsync();

        var priceMap = lastPrices.ToDictionary(x => x.IdProdus, x => x.LastPrice);

        var productsQuery =
            from produs in _context.Products.Where(p => p.Valabil && p.IdUser != null)
            join user in _context.Users on produs.IdUser equals user.IdUser into userGroup
            from user in userGroup.DefaultIfEmpty() 
            join persoana in _context.Persoane on user.IdPersoana equals persoana.IdPersoana into persoanaGroup
            from persoana in persoanaGroup.DefaultIfEmpty()
            join categorie in _context.Categorii on produs.IdCategorie equals categorie.IdCategorie
            join imagine in _context.Imagini on produs.IdProdus equals imagine.IdProdus into imgGroup
            from imagine in imgGroup.DefaultIfEmpty()
            select new { produs, user, persoana, categorie, imagine };

        var resultList = await productsQuery.ToListAsync();

        var list = resultList.Select(x => new ProdusDTO
        {
            ProdusId = x.produs.IdProdus,
            Nume = (x.produs.Nume ?? string.Empty).Trim(),
            Descriere = (x.produs.Descriere ?? string.Empty).Trim(),
            EsteSprayText = x.produs.EsteSpray ? "DA" : "NU",
            Imagine = x.imagine != null ? x.imagine.Fisier : null,
            Pret = priceMap.TryGetValue(x.produs.IdProdus, out var p) ? p : 0,
            Valabil = x.produs.Valabil,
            Categorie = x.categorie.DenumireCategorie,
            Vendor = x.persoana == null
                ? "Necunoscut"
                : (x.persoana.Rol == "Owner" || x.persoana.Rol == "Administrator")
                    ? "SC AUTO PAINTS SRL"
                    : (x.persoana.tipPersoana == "Fizica"
                        ? throw new InvalidOperationException("EROARE: PERSOANA ESTE DE TIP FIZIC.")
                        : $"{x.persoana.Nume.Trim()} {x.persoana.Prenume.Trim()}")
        }).ToList();

        return list;
    }




    public async Task<IEnumerable<ProdusDTO>> GetOfferedProductsAsync()
    {
        var now = DateTime.UtcNow; // data curenta


        var query =
            from produs in _context.Products
            join user in _context.Users on produs.IdUser equals user.IdUser into pu
            from user in pu.DefaultIfEmpty()
            join persoana in _context.Persoane on user.IdPersoana equals persoana.IdPersoana into up
            from persoana in up.DefaultIfEmpty()
            join categorie in _context.Categorii on produs.IdCategorie equals categorie.IdCategorie
            join imagine in _context.Imagini on produs.IdProdus equals imagine.IdProdus into imgGroup
            from imagine in imgGroup.DefaultIfEmpty()

                // Filtram doar produsele care au un prer cu DataExpirare > now
            join pretGroup in _context.Preturi_Produs
                .Where(pp => pp.DataExpirare != null && pp.DataExpirare > now)
                .GroupBy(p => p.IdProdus)
                .Select(g => g.OrderByDescending(p => p.DataInceput).FirstOrDefault())
            on produs.IdProdus equals pretGroup.IdProdus

            select new ProdusDTO
            {
                ProdusId = produs.IdProdus,
                Nume = (produs.Nume ?? string.Empty).Trim(),
                Descriere = (produs.Descriere ?? string.Empty).Trim(),
                EsteSprayText = produs.EsteSpray ? "DA" : "NU",
                Imagine = imagine != null ? imagine.Fisier : null,
                Pret = pretGroup.Pret, 
                Valabil = produs.Valabil,
                Categorie = categorie.DenumireCategorie,
                Vendor = (user != null && user.Username.ToLower() == "admin")
                    ? "SC AUTOPAINTS SRL"
                    : (persoana != null
                        ? $"{persoana.Nume.Trim()} {persoana.Prenume.Trim()}"
                        : "Necunoscut")
            };

        return await query.ToListAsync();
    }



}
