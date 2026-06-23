using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.DTOs;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;

namespace LicentaInAngular.Server.Repositories
{
    public class ProductService : IProductRepository
    {
        private readonly ApplicationDbContext _context;

        public ProductService(ApplicationDbContext context)
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
                    .GroupBy(pp => pp.IdProdus)
                    .Select(g => g.OrderByDescending(pp => pp.DataInceput).FirstOrDefault())
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

        public async Task<IEnumerable<ProdusDTO>> GetProdusDTOsAsync()
        {
            var lastPrices = await _context.Preturi_Produs
                .GroupBy(x => x.IdProdus)
                .Select(g => new
                {
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
            var now = DateTime.UtcNow;

            var query =
                from produs in _context.Products
                join user in _context.Users on produs.IdUser equals user.IdUser into pu
                from user in pu.DefaultIfEmpty()
                join persoana in _context.Persoane on user.IdPersoana equals persoana.IdPersoana into up
                from persoana in up.DefaultIfEmpty()
                join categorie in _context.Categorii on produs.IdCategorie equals categorie.IdCategorie
                join imagine in _context.Imagini on produs.IdProdus equals imagine.IdProdus into imgGroup
                from imagine in imgGroup.DefaultIfEmpty()

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

        public async Task<IActionResult> GetProductsResponse()
        {
            var products = await GetAll();
            return new OkObjectResult(products);
        }

        public async Task<IActionResult> GetProductsByCategoryResponse(string category)
        {
            var products = await GetByCategory(category);

            if (products == null || !products.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products found in this category."
                });
            }

            return new OkObjectResult(products);
        }

        public async Task<IActionResult> GetProductsByCategoriesResponse(List<string> categories)
        {
            if (categories == null || !categories.Any())
            {
                return new BadRequestObjectResult(new
                {
                    message = "Please provide at least one category."
                });
            }

            var categoryList = categories.Select(c => c.Trim().ToLower()).ToList();

            var products = await GetByCategories(categoryList);

            if (products == null || !products.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products found in the specified categories."
                });
            }

            return new OkObjectResult(products);
        }

        public async Task<IActionResult> GetProductsByUserIdResponse(int userId)
        {
            var products = await GetByUserId(userId);

            if (products == null || !products.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products found for this user."
                });
            }

            return new OkObjectResult(products);
        }

        public async Task<IActionResult> GetProductResponse(int id)
        {
            var product = await GetById(id);

            if (product == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Product not found."
                });
            }

            return new OkObjectResult(product);
        }

        public async Task<IActionResult> SearchProductsByNameResponse(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return new BadRequestObjectResult(new
                {
                    message = "Search term cannot be empty."
                });
            }

            var products = await SearchByName(name);

            if (products == null || !products.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products match the search criteria."
                });
            }

            return new OkObjectResult(products);
        }

        public async Task<IActionResult> CreateProductResponse(ProductUploadDto productDto)
        {
            try
            {
                byte[] imageBytes = null;

                if (productDto.Imagine != null)
                {
                    using (var ms = new MemoryStream())
                    {
                        await productDto.Imagine.CopyToAsync(ms);
                        imageBytes = ms.ToArray();
                    }
                }

                var categorieEntity = await _context.Categorii
                    .FirstOrDefaultAsync(c => c.DenumireCategorie == productDto.Categorie);

                if (categorieEntity == null)
                {
                    return new BadRequestObjectResult(new
                    {
                        message = "Categoria specificata nu exista."
                    });
                }

                var product = new Produs
                {
                    Nume = productDto.Nume,
                    Descriere = productDto.Descriere,
                    EsteSpray = productDto.EsteSpray,
                    Valabil = productDto.Valabil,
                    IdCategorie = categorieEntity.IdCategorie,
                    IdUser = productDto.IdUser
                };

                _context.Products.Add(product);
                await _context.SaveChangesAsync();

                if (productDto.IdUser == 1)
                {
                    var pretProdus = new Preturi_Produs
                    {
                        IdProdus = product.IdProdus,
                        Pret = productDto.Pret,
                        DataInceput = DateTime.UtcNow,
                        Comision = null
                    };

                    _context.Preturi_Produs.Add(pretProdus);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    var pretProdus = new Preturi_Produs
                    {
                        IdProdus = product.IdProdus,
                        Pret = productDto.Pret,
                        DataInceput = DateTime.UtcNow,
                        Comision = 10m
                    };

                    _context.Preturi_Produs.Add(pretProdus);
                    await _context.SaveChangesAsync();
                }

                if (imageBytes != null)
                {
                    var imagineEntity = new Imagini
                    {
                        IdProdus = product.IdProdus,
                        Fisier = imageBytes
                    };

                    _context.Imagini.Add(imagineEntity);
                    await _context.SaveChangesAsync();
                }

                if (!string.IsNullOrEmpty(productDto.CodCuloare))
                {
                    var vopsea = new Vopsea
                    {
                        IdProdus = product.IdProdus,
                        CodCuloare = productDto.CodCuloare
                    };

                    _context.Vopsele.Add(vopsea);
                    await _context.SaveChangesAsync();
                }

                return new CreatedAtActionResult(
                    "GetProduct",
                    "Product",
                    new { id = product.IdProdus },
                    new
                    {
                        message = "Produs creat cu succes!",
                        IdProdus = product.IdProdus
                    }
                );
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    message = "Eroare la crearea produsului.",
                    error = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> UpdateProductResponse(int id, Produs product)
        {
            if (id != product.IdProdus)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Product ID mismatch."
                });
            }

            var existingProduct = await GetById(id);

            if (existingProduct == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Product not found."
                });
            }

            await UpdateProduct(product);

            return new NoContentResult();
        }

        public async Task<IActionResult> DeleteProductResponse(int id)
        {
            var product = await GetById(id);

            if (product == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Product not found."
                });
            }

            await DeleteProduct(id);

            return new NoContentResult();
        }

        public async Task<IActionResult> GetDetailedProductsResponse()
        {
            var productsDto = await GetProdusDTOsAsync();

            if (productsDto == null || !productsDto.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products found."
                });
            }

            return new OkObjectResult(productsDto);
        }

        public async Task<IActionResult> GetOfferedProductsResponse()
        {
            var offeredProds = await GetOfferedProductsAsync();

            if (offeredProds == null || !offeredProds.Any())
            {
                return new NotFoundObjectResult(new
                {
                    message = "No products in offer."
                });
            }

            return new OkObjectResult(offeredProds);
        }

        public async Task<IActionResult> AdaugaReducereResponse(Reducere_DTO dto)
        {
            if (dto == null || dto.IdProdus <= 0 || dto.PretNou <= 0)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Date reducere invalide!"
                });
            }

            var produs = await _context.Products.FindAsync(dto.IdProdus);

            if (produs == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Produsul nu exista!"
                });
            }

            var acum = DateTime.UtcNow;

            var reducereActiva = await _context.Preturi_Produs
                .AnyAsync(p => p.IdProdus == dto.IdProdus && p.DataExpirare != null && p.DataExpirare > acum);

            if (reducereActiva)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Exista deja o reducere activa pentru acest produs!"
                });
            }

            var pretReducere = new Preturi_Produs
            {
                IdProdus = dto.IdProdus,
                Pret = dto.PretNou,
                DataInceput = DateTime.UtcNow,
                DataExpirare = dto.DataExpirare ?? DateTime.UtcNow.AddDays(7),
                Comision = null
            };

            _context.Preturi_Produs.Add(pretReducere);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Reducere aplicata cu succes!"
            });
        }

        public async Task<IActionResult> AfisareToatePreturileDinBDResponse()
        {
            var preturi = await _context.Preturi_Produs
                .Select(p => new
                {
                    p.idPP,
                    p.IdProdus,
                    p.Pret,
                    p.DataInceput,
                    p.DataExpirare
                })
                .ToListAsync();

            return new OkObjectResult(preturi);
        }

        public async Task<IActionResult> AfiseazaReducereaResponse(int idProdus)
        {
            var preturi = await _context.Preturi_Produs
                .Where(p => p.IdProdus == idProdus)
                .OrderByDescending(p => p.DataInceput)
                .ToListAsync();

            var pretVechi = preturi.FirstOrDefault(p => p.DataExpirare == null);

            var pretNou = preturi
                .Where(p => p.DataExpirare != null)
                .OrderByDescending(p => p.DataExpirare)
                .FirstOrDefault();

            if (pretVechi == null && pretNou == null)
            {
                return new NotFoundObjectResult("Nu exista preturi pentru acest produs.");
            }

            return new OkObjectResult(new
            {
                pretVechi = pretVechi != null ? new
                {
                    pretVechi.Pret,
                    pretVechi.DataInceput,
                    pretVechi.DataExpirare
                } : null,
                pretNou = pretNou != null ? new
                {
                    pretNou.Pret,
                    pretNou.DataInceput,
                    pretNou.DataExpirare
                } : null
            });
        }

        public async Task<IActionResult> GetProduseCuReducereResponse()
        {
            var preturiReduceri = await _context.Preturi_Produs
                .GroupBy(p => p.IdProdus)
                .Select(g => new
                {
                    IdProdus = g.Key,
                    PretVechi = g.Where(x => x.DataExpirare == null)
                                 .OrderByDescending(x => x.DataInceput)
                                 .Select(x => x.Pret)
                                 .FirstOrDefault(),
                    PretReducere = g.Where(x => x.DataExpirare != null)
                                    .OrderByDescending(x => x.DataInceput)
                                    .Select(x => x.Pret)
                                    .FirstOrDefault()
                })
                .Where(x => x.PretReducere > 0 && x.PretVechi > 0 && x.PretReducere < x.PretVechi)
                .ToListAsync();

            var iduriProduseReducere = preturiReduceri.Select(x => x.IdProdus).ToList();

            var produse = await (
                from p in _context.Products
                where iduriProduseReducere.Contains(p.IdProdus)
                join cat in _context.Categorii on p.IdCategorie equals cat.IdCategorie
                join user in _context.Users on p.IdUser equals user.IdUser into userGroup
                from user in userGroup.DefaultIfEmpty()
                join persoana in _context.Persoane on user.IdPersoana equals persoana.IdPersoana into persoanaGroup
                from persoana in persoanaGroup.DefaultIfEmpty()
                join img in _context.Imagini on p.IdProdus equals img.IdProdus into imgGroup
                from img in imgGroup.DefaultIfEmpty()
                select new
                {
                    IdProdus = p.IdProdus,
                    Nume = p.Nume,
                    Descriere = p.Descriere,
                    Imagine = img != null ? img.Fisier : null,
                    Categorie = cat.DenumireCategorie,
                    Vendor = persoana != null
                        ? (persoana.Rol == "Owner" || persoana.Rol == "Administrator" ? "SC AUTO PAINTS SRL" : $"{persoana.Nume} {persoana.Prenume}")
                        : "Necunoscut"
                }).ToListAsync();

            var rezultatFinal = produse.Select(p =>
            {
                var pret = preturiReduceri.First(x => x.IdProdus == p.IdProdus);

                return new
                {
                    p.IdProdus,
                    p.Nume,
                    p.Descriere,
                    p.Imagine,
                    p.Categorie,
                    p.Vendor,
                    PretVechi = pret.PretVechi,
                    PretReducere = pret.PretReducere
                };
            });

            return new OkObjectResult(rezultatFinal);
        }

        public async Task<IActionResult> AdminUpdateProdusResponse(int idProdus, AdminUpdateProdus_DTO dto)
        {
            var produs = await _context.Products.FindAsync(idProdus);

            if (produs == null)
            {
                return new NotFoundObjectResult("Produsul nu exista.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Nume))
                produs.Nume = dto.Nume;

            if (!string.IsNullOrWhiteSpace(dto.Descriere))
                produs.Descriere = dto.Descriere;

            if (dto.EsteSpray.HasValue)
                produs.EsteSpray = dto.EsteSpray.Value;

            if (dto.Valabil.HasValue)
                produs.Valabil = dto.Valabil.Value;

            if (dto.IdUser.HasValue)
                produs.IdUser = dto.IdUser.Value;

            if (!string.IsNullOrWhiteSpace(dto.DenumireCategorie))
            {
                string denumireCat = dto.DenumireCategorie.Trim().ToLower();

                var categorie = await _context.Categorii
                    .FirstOrDefaultAsync(c => c.DenumireCategorie.ToLower() == denumireCat);

                if (categorie == null)
                {
                    categorie = new Categorii
                    {
                        DenumireCategorie = denumireCat,
                        DescriereCategorie = "-"
                    };

                    _context.Categorii.Add(categorie);
                    await _context.SaveChangesAsync();
                }

                produs.IdCategorie = categorie.IdCategorie;
            }

            if (!string.IsNullOrEmpty(dto.ImagineBase64))
            {
                var imagine = await _context.Imagini
                    .FirstOrDefaultAsync(img => img.IdProdus == idProdus);

                byte[] fisierImg = Convert.FromBase64String(
                    dto.ImagineBase64.Replace("data:image/png;base64,", "")
                                     .Replace("data:image/jpeg;base64,", "")
                );

                if (imagine != null)
                {
                    imagine.Fisier = fisierImg;
                }
                else
                {
                    _context.Imagini.Add(new Imagini
                    {
                        IdProdus = idProdus,
                        Fisier = fisierImg
                    });
                }
            }

            if (dto.Cantitate.HasValue)
            {
                var subproduseValabile = await _context.SubProduse
                    .Where(sp => sp.IdProdus == idProdus && sp.Valabil)
                    .ToListAsync();

                int diferenta = dto.Cantitate.Value - subproduseValabile.Count;

                if (diferenta > 0)
                {
                    for (int i = 0; i < diferenta; i++)
                    {
                        _context.SubProduse.Add(new Subprodus
                        {
                            IdProdus = idProdus,
                            Valabil = true,
                            idCos = null
                        });
                    }
                }
                else if (diferenta < 0)
                {
                    var deDezactivat = subproduseValabile.Take(-diferenta).ToList();

                    foreach (var sp in deDezactivat)
                    {
                        sp.Valabil = false;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Produsul a fost actualizat cu succes!"
            });
        }

        public async Task<IActionResult> StergePretResponse(int idPP)
        {
            var pret = await _context.Preturi_Produs.FindAsync(idPP);

            if (pret == null)
            {
                return new NotFoundObjectResult(new
                {
                    mesaj = "Pretul nu a fost gasit."
                });
            }

            _context.Preturi_Produs.Remove(pret);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                mesaj = "Pretul a fost sters cu succes."
            });
        }

        public async Task<IActionResult> GetPreturiPentruProdusResponse(int idProdus)
        {
            var preturi = await _context.Preturi_Produs
                .Where(p => p.IdProdus == idProdus)
                .OrderByDescending(p => p.DataInceput)
                .ToListAsync();

            if (preturi == null || !preturi.Any())
            {
                return new NotFoundObjectResult(new
                {
                    mesaj = "Nu exista preturi pentru acest produs."
                });
            }

            return new OkObjectResult(preturi);
        }

        public async Task<IActionResult> ModificaPretResponse(int idPP, UpdatePret_DTO dto)
        {
            var pret = await _context.Preturi_Produs.FindAsync(idPP);

            if (pret == null)
            {
                return new NotFoundObjectResult(new
                {
                    mesaj = "Pretul nu a fost gasit."
                });
            }

            pret.Pret = dto.Pret;

            if (dto.Comision.HasValue)
                pret.Comision = dto.Comision;

            if (dto.DataExpirare.HasValue)
                pret.DataExpirare = dto.DataExpirare;

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                mesaj = "Pretul a fost actualizat cu succes."
            });
        }

        public async Task<IActionResult> DezactiveazaProdusResponse(int idProdus)
        {
            var produs = await _context.Products.FindAsync(idProdus);

            if (produs == null)
            {
                return new NotFoundObjectResult($"Produsul cu ID {idProdus} nu a fost gasit.");
            }

            produs.Valabil = false;
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Produsul a fost dezactivat cu succes."
            });
        }
    }
}