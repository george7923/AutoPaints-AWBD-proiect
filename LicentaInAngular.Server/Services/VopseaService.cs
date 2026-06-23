using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public class VopseaService : IVopseaRepository
    {
        private readonly ApplicationDbContext _context;

        public VopseaService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CreareVopseaSiAdaugaInCosResponse(CreateVopseaSiAdaugaInCosDto dto)
        {
            try
            {
                // 1. Cos
                var existingCart = await _context.Cosuri.FirstOrDefaultAsync(c => c.IdUser == dto.IdUser);

                if (existingCart == null)
                {
                    existingCart = new Cos
                    {
                        CodUnic = Guid.NewGuid().ToString(),
                        IdUser = dto.IdUser,
                        DataCreare = DateTime.UtcNow
                    };

                    _context.Cosuri.Add(existingCart);
                    await _context.SaveChangesAsync();
                }

                // 2. Categorie
                string categorieText = dto.TipVopsea.ToLower() == "spray"
                    ? "spray vopsea auto preparat dupa cod"
                    : "vopsea auto preparata dupa codul de culoare al masinii";

                var categorieEntity = await _context.Categorii
                    .FirstOrDefaultAsync(c => c.DenumireCategorie.ToLower() == categorieText.ToLower());

                if (categorieEntity == null)
                {
                    categorieEntity = new Categorii
                    {
                        DenumireCategorie = categorieText,
                        DescriereCategorie = "Creată automat pentru vopsele personalizate"
                    };

                    _context.Categorii.Add(categorieEntity);
                    await _context.SaveChangesAsync();
                }

                // 3. Nume unic produs
                string numeDeBaza = $"{dto.TipVopsea.ToLower()} auto personalizata".Trim().ToLower();

                var produseSimilare = await _context.Products
                    .Where(p => p.Nume.ToLower().StartsWith(numeDeBaza))
                    .Select(p => p.Nume.ToLower())
                    .ToListAsync();

                int sufix = 1;
                string numeFinal = numeDeBaza;

                while (produseSimilare.Contains(numeFinal))
                {
                    sufix++;
                    numeFinal = $"{numeDeBaza} ({sufix})";
                }

                // 4. Produs
                var produsNou = new Produs
                {
                    Nume = numeFinal,
                    Descriere = $"{dto.Model} {dto.An} {dto.SerieCaroserie}. Detalii: {dto.DetaliiSuplimentare}.",
                    EsteSpray = true,
                    Valabil = true,
                    IdCategorie = categorieEntity.IdCategorie
                };

                _context.Products.Add(produsNou);
                await _context.SaveChangesAsync();

                // 5. Pret
                var pretProdus = new Preturi_Produs
                {
                    IdProdus = produsNou.IdProdus,
                    Pret = dto.TipVopsea.ToLower() == "spray" ? 60 : 35,
                    DataInceput = DateTime.UtcNow
                };

                _context.Preturi_Produs.Add(pretProdus);
                await _context.SaveChangesAsync();

                // 6. Marca
                var marcaTrim = dto.MarcaMasinii?.Trim().ToLower();

                var marcaEntity = await _context.Marci
                    .FirstOrDefaultAsync(m => m.NumeMarca.ToLower() == marcaTrim);

                if (marcaEntity == null)
                {
                    marcaEntity = new Marca
                    {
                        NumeMarca = dto.MarcaMasinii.Trim()
                    };

                    _context.Marci.Add(marcaEntity);
                    await _context.SaveChangesAsync();
                }

                // 7. Model
                var modelTrim = dto.Model?.Trim().ToLower();

                var modelEntity = await _context.Modele
                    .FirstOrDefaultAsync(m =>
                        m.NumeModel.ToLower() == modelTrim &&
                        m.IdMarca == marcaEntity.IdMarca &&
                        m.An == dto.An);

                if (modelEntity == null)
                {
                    modelEntity = new Model
                    {
                        NumeModel = dto.Model.Trim(),
                        IdMarca = marcaEntity.IdMarca,
                        An = dto.An
                    };

                    _context.Modele.Add(modelEntity);
                    await _context.SaveChangesAsync();
                }

                // 8. Vopsea cu IdModel
                var vopseaNoua = new Vopsea
                {
                    TipVopsea = dto.TipVopsea,
                    CodCuloare = dto.CodCuloare,
                    SerieCaroserie = dto.SerieCaroserie,
                    IdProdus = produsNou.IdProdus,
                    IdModel = modelEntity.IdModel
                };

                _context.Vopsele.Add(vopseaNoua);
                await _context.SaveChangesAsync();

                // 9. Subproduse
                var subproduseList = new List<Subprodus>();

                for (int i = 0; i < dto.Cantitate; i++)
                {
                    subproduseList.Add(new Subprodus
                    {
                        IdProdus = produsNou.IdProdus,
                        Valabil = true,
                        idCos = existingCart.idCos
                    });
                }

                if (subproduseList.Any())
                {
                    _context.SubProduse.AddRange(subproduseList);
                    await _context.SaveChangesAsync();
                }

                // 10. DTO final produs
                var sablonProdus = await (
                    from produs in _context.Products
                    join cat in _context.Categorii on produs.IdCategorie equals cat.IdCategorie
                    join pret in _context.Preturi_Produs on produs.IdProdus equals pret.IdProdus into pretGroup
                    from pret in pretGroup.OrderByDescending(p => p.DataInceput).Take(1).DefaultIfEmpty()
                    join img in _context.Imagini on produs.IdProdus equals img.IdProdus into imgGroup
                    from img in imgGroup.DefaultIfEmpty()
                    where produs.IdProdus == produsNou.IdProdus
                    select new SablonProdusDTO
                    {
                        IdProdus = produs.IdProdus,
                        Nume = produs.Nume,
                        Descriere = produs.Descriere,
                        EsteSpray = produs.EsteSpray,
                        Imagine = img != null ? img.Fisier : null,
                        Pret = pret != null ? pret.Pret : 0,
                        Valabil = produs.Valabil,
                        Categorie = cat.DenumireCategorie,
                        IdUser = produs.IdUser,
                        User = produs.User
                    }
                ).FirstOrDefaultAsync();

                // 11. Raspuns
                return new OkObjectResult(new
                {
                    message = "Vopsea + Produs + Subproduse create și adăugate în coș.",
                    Produs = sablonProdus,
                    IdVopsea = vopseaNoua.idVopsea,
                    IdCos = existingCart.idCos,
                    SubproduseCreateInCos = subproduseList.Count
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    message = "Eroare la crearea vopselei/produsului + adăugare în coș.",
                    error = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}