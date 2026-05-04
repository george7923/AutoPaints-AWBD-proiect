using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.DTOs;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.DataLayer.Models;
using LicentaInAngular.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProductRepository _productRepository;
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public ProductController(IProductRepository productRepository, IUserRepository userRepository, ApplicationDbContext context)
        {
            _productRepository = productRepository;
            _userRepository = userRepository;
            _context = context;

        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<Produs>>> GetProducts()
        {
            var products = await _productRepository.GetAll();
            return Ok(products);
        }


        [HttpGet("category/{category}")]
        public async Task<ActionResult<IEnumerable<Produs>>> GetProductsByCategory(string category)
        {
            var products = await _productRepository.GetByCategory(category);
            if (products == null || !products.Any())
            {
                return NotFound(new { message = "No products found in this category." });
            }
            return Ok(products);
        }

        [HttpGet("categories")]
        public async Task<ActionResult<IEnumerable<Produs>>> GetProductsByCategories([FromQuery] List<string> categories)
        {
            if (categories == null || !categories.Any())
            {
                return BadRequest(new { message = "Please provide at least one category." });
            }


            var categoryList = categories.Select(c => c.Trim().ToLower()).ToList();

            var products = await _productRepository.GetByCategories(categoryList);

            if (products == null || !products.Any())
            {
                return NotFound(new { message = "No products found in the specified categories." });
            }

            return Ok(products);
        }


        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Produs>>> GetProductsByUserId(int userId)
        {
            var products = await _productRepository.GetByUserId(userId);
            if (products == null || !products.Any())
            {
                return NotFound(new { message = "No products found for this user." });
            }
            return Ok(products);
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<Produs>> GetProduct(int id)
        {
            var product = await _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }
            return Ok(product);
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Produs>>> SearchProductsByName([FromQuery] string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return BadRequest(new { message = "Search term cannot be empty." });
            }

            var products = await _productRepository.SearchByName(name);

            if (products == null || !products.Any())
            {
                return NotFound(new { message = "No products match the search criteria." });
            }

            return Ok(products);
        }


        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Produs>> CreateProduct([FromForm] ProductUploadDto productDto)
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
                    return BadRequest(new { message = "Categoria specificată nu există." });
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
                        Comision = 10m // Comision de 10% pentru alti utilizatori
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

                return CreatedAtAction(nameof(GetProduct), new { id = product.IdProdus }, new
                {
                    message = "Produs creat cu succes!",
                    IdProdus = product.IdProdus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "Eroare la crearea produsului.",
                    error = ex.Message
                });
            }
        }



        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, Produs product)
        {
            if (id != product.IdProdus)
            {
                return BadRequest(new { message = "Product ID mismatch." });
            }

            var existingProduct = await _productRepository.GetById(id);
            if (existingProduct == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            await _productRepository.UpdateProduct(product);
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _productRepository.GetById(id);
            if (product == null)
            {
                return NotFound(new { message = "Product not found." });
            }

            await _productRepository.DeleteProduct(id);
            return NoContent();
        }

        [HttpGet("detailed")]
        public async Task<ActionResult<IEnumerable<ProdusDTO>>> GetDetailedProducts()
        {
            var productsDto = await _productRepository.GetProdusDTOsAsync();
            if (productsDto == null || !productsDto.Any())
            {
                return NotFound(new { message = "No products found." });
            }
            return Ok(productsDto);
        }


        [HttpGet("offer")]
        public async Task<ActionResult<IEnumerable<ProdusDTO>>> GetOfferedProducts()
        {
            var offeredProds = await _productRepository.GetOfferedProductsAsync();
            if (offeredProds == null || !offeredProds.Any())
            {
                return NotFound(new { message = "No products in offer." });
            }
            return Ok(offeredProds);
        }
        [HttpPost("reducere")]
        public async Task<IActionResult> AdaugaReducere([FromBody] ReducereDTO dto)
        {

            if (dto == null || dto.IdProdus <= 0 || dto.PretNou <= 0)
                return BadRequest(new { message = "Date reducere invalide!" });

            var produs = await _context.Products.FindAsync(dto.IdProdus);
            if (produs == null)
                return NotFound(new { message = "Produsul nu există!" });

            var acum = DateTime.UtcNow;
            var reducereActiva = await _context.Preturi_Produs
                .AnyAsync(p => p.IdProdus == dto.IdProdus && p.DataExpirare != null && p.DataExpirare > acum);

            if (reducereActiva)
                return BadRequest(new { message = "Există deja o reducere activă pentru acest produs!" });
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

            return Ok(new { message = "Reducere aplicată cu succes!" });
        }

        [HttpGet("toate-preturile")]
        public async Task<IActionResult> AfisareToatePreturileDinBD()
        {
            var preturi = await _context.Preturi_Produs
                .Select(p => new {
                    p.idPP,
                    p.IdProdus,
                    p.Pret,
                    p.DataInceput,
                    p.DataExpirare
                })
                .ToListAsync();

            return Ok(preturi);
        }
        [HttpGet("reducere/{idProdus}")]
        public async Task<IActionResult> AfiseazaReducerea(int idProdus)
        {
            var preturi = await _context.Preturi_Produs
                .Where(p => p.IdProdus == idProdus)
                .OrderByDescending(p => p.DataInceput)
                .ToListAsync();

            // PRET VECHI: PRETUL CU DataExpirare == NULL si DataInceput CEA MAI RECENTA
            var pretVechi = preturi.FirstOrDefault(p => p.DataExpirare == null);

            // PRET NOU: PRETUL CU DataExpirare != NULL si DataExpirare CEA MAI RECENTA
            var pretNou = preturi
                .Where(p => p.DataExpirare != null)
                .OrderByDescending(p => p.DataExpirare)
                .FirstOrDefault();

            if (pretVechi == null && pretNou == null)
                return NotFound("Nu există prețuri pentru acest produs.");

            return Ok(new
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
        [HttpGet("cu-reducere")]
        public async Task<IActionResult> GetProduseCuReducere()
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

            return Ok(rezultatFinal);
        }


        [HttpPut("admin-update/{idProdus}")]
        public async Task<IActionResult> AdminUpdateProdus(int idProdus, [FromBody] AdminUpdateProdusDTO dto)
        {

            var produs = await _context.Products.FindAsync(idProdus);
            if (produs == null)
                return NotFound("Produsul nu există.");


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
                var imagine = await _context.Imagini.FirstOrDefaultAsync(img => img.IdProdus == idProdus);
                byte[] fisierImg = Convert.FromBase64String(
                    dto.ImagineBase64.Replace("data:image/png;base64,", "")
                                     .Replace("data:image/jpeg;base64,", "")
                );
                if (imagine != null)
                    imagine.Fisier = fisierImg;
                else
                    _context.Imagini.Add(new Imagini { IdProdus = idProdus, Fisier = fisierImg });
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
            return Ok(new { message = "Produsul a fost actualizat cu succes!" });
        }


        [HttpDelete("sterge-pret/{idPP}")]
        public async Task<IActionResult> StergePret(int idPP)
        {
            var pret = await _context.Preturi_Produs.FindAsync(idPP);

            if (pret == null)
                return NotFound(new { mesaj = "Prețul nu a fost găsit." });

            _context.Preturi_Produs.Remove(pret);
            await _context.SaveChangesAsync();

            return Ok(new { mesaj = "Prețul a fost șters cu succes." });
        }
        [HttpGet("preturi/{idProdus}")]
        public async Task<IActionResult> GetPreturiPentruProdus(int idProdus)
        {
            var preturi = await _context.Preturi_Produs
                .Where(p => p.IdProdus == idProdus)
                .OrderByDescending(p => p.DataInceput)
                .ToListAsync();

            if (preturi == null || !preturi.Any())
                return NotFound(new { mesaj = "Nu există prețuri pentru acest produs." });

            return Ok(preturi);
        }
        [HttpPut("modifica-pret/{idPP}")]
        public async Task<IActionResult> ModificaPret(int idPP, [FromBody] UpdatePretDTO dto)
        {
            var pret = await _context.Preturi_Produs.FindAsync(idPP);
            if (pret == null)
                return NotFound(new { mesaj = "Prețul nu a fost găsit." });

            pret.Pret = dto.Pret;

            if (dto.Comision.HasValue)
                pret.Comision = dto.Comision;

            if (dto.DataExpirare.HasValue)
                pret.DataExpirare = dto.DataExpirare;

            await _context.SaveChangesAsync();

            return Ok(new { mesaj = "Prețul a fost actualizat cu succes." });
        }
        [HttpPut("dezactiveaza/{idProdus}")]
        public async Task<IActionResult> DezactiveazaProdus(int idProdus)
        {
            var produs = await _context.Products.FindAsync(idProdus);
            if (produs == null)
                return NotFound($"Produsul cu ID {idProdus} nu a fost găsit.");

            produs.Valabil = false;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Produsul a fost dezactivat cu succes." });
        }

        
    }
    public class ReducereDTO
    {
        public int IdProdus { get; set; }
        public decimal PretNou { get; set; }
        public DateTime? DataExpirare { get; set; }
    }

    public class AdminUpdateProdusDTO
    {
        public int? IdProdus { get; set; }
        public string? Nume { get; set; }
        public string? Descriere { get; set; }
        public bool? EsteSpray { get; set; }
        public bool? Valabil { get; set; }
        public string? DenumireCategorie { get; set; }   
        public int? IdUser { get; set; }
        public string? ImagineBase64 { get; set; }
        public int? Cantitate { get; set; }
    }

    public class UpdatePretDTO
    {
        public decimal Pret { get; set; }
        public decimal? Comision { get; set; }
        public DateTime? DataExpirare { get; set; }
    }

}
