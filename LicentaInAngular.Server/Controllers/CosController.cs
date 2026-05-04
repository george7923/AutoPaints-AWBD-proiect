using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Data;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace LicentaInAngular.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CosController : ControllerBase
    {
        private readonly ICosRepository _cosRepository;
        private readonly ApplicationDbContext _context;
        private readonly string _errorInvalidRequest = "Invalid request data.";

        public CosController(ICosRepository cosRepository, ApplicationDbContext context)
        {
            _cosRepository = cosRepository;
            _context = context;
        }

        // GET: api/cos/user/{userId}
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetCartByUserId(int userId)
        {
            var cart = await _cosRepository.GetCartByUserId(userId);
            if (cart == null)
            {
                return NotFound(new { message = "Cart not found for the given user." });
            }
            return Ok(cart);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCartById(int id)
        {
            var cart = await _context.Cosuri.FindAsync(id);
            if (cart == null)
                return NotFound();

            return Ok(cart);
        }

        [HttpPost("check-or-create/{userId}")]
        public async Task<IActionResult> CheckOrCreateCart(int userId)
        {
            if (userId <= 0)
                return BadRequest(new { message = "Invalid user ID." });

            var user = await _context.Users.FindAsync(userId);
            if (user == null)
                return NotFound(new { message = "Utilizator inexistent.Nu se poate crea un coș." });

            var existingCart = await _cosRepository.GetCartByUserId(userId);
            if (existingCart != null)
            {
                return Conflict(new
                {
                    message = "Utilizatorul are deja un coș.",
                    cartId = existingCart.idCos
                });
            }

            var newCart = new Cos
            {
                IdUser = userId,
                CodUnic = "cart-" + DateTime.UtcNow.Ticks,
                DataCreare = DateTime.UtcNow
            };

            try
            {
                _context.Cosuri.Add(newCart);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetCartByUserId),
                    new { id = newCart.idCos },
                    newCart);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Eroare internă la creare coș.", detail = ex.Message });
            }
        }




        [HttpGet("cart-details/{userId}")]
        public async Task<IActionResult> GetCartDetailsByUser(int userId)
        {
            var cart = await _cosRepository.GetCartByUserId(userId);
            if (cart == null)
            {
                return NotFound(new { message = "Cart not found for the given user." });
            }

            // Summarize subproducts by product
            var items = await _context.SubProduse
                .Where(sp => sp.idCos == cart.idCos)
                .GroupBy(sp => sp.IdProdus)
                .Select(g => new
                {
                    IdProdus = g.Key,
                    Quantity = g.Count(),
                    Produs = _context.Products.FirstOrDefault(p => p.IdProdus == g.Key)
                })
                .ToListAsync();

            return Ok(new { Cart = cart, Items = items });
        }




        [HttpPut("add-one")]
        public async Task<IActionResult> AddOneSubprodusToCart([FromBody] SubprodusUpdateDTO request)
        {
            if (request == null || request.IdProdus <= 0 || request.idCos <= 0)
            {
                return BadRequest(new { message = _errorInvalidRequest });
            }


            return await UpdateCartSubprodus(request, "add-one");
        }


        [HttpPut("remove-one")]
        public async Task<IActionResult> RemoveOneSubprodusFromCart([FromBody] SubprodusUpdateDTO request)
        {
            if (request == null || request.IdProdus <= 0 || request.idCos <= 0)
            {
                return BadRequest(new { message = _errorInvalidRequest });
            }

            return await UpdateCartSubprodus(request, "remove-one");
        }


        [HttpPut("remove-all")]
        public async Task<IActionResult> RemoveAllSubproduseFromCart([FromBody] SubprodusUpdateDTO request)
        {
            if (request == null || request.IdProdus <= 0 || request.idCos <= 0)
            {
                return BadRequest(new { message = _errorInvalidRequest });
            }

            return await UpdateCartSubprodus(request, "remove-all");
        }


        private async Task<IActionResult> UpdateCartSubprodus(SubprodusUpdateDTO request, string operation)
        {

            switch (operation)
            {
                case "add-one":
                    // Cautam un subprodus orfan (idCos == null) pentru produsul cerut
                    var subToAdd = await _context.SubProduse
                        .Where(sp => sp.IdProdus == request.IdProdus && sp.idCos == null)
                        .OrderBy(sp => sp.IdSubprodus)
                        .FirstOrDefaultAsync();

                    if (subToAdd == null)
                        return NotFound(new { message = "No available subproduct for this product." });

                    // Asignam subprodusul la cos
                    subToAdd.idCos = request.idCos;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "One subproduct added to cart." });

                case "remove-one":
                    var subToRemove = await _context.SubProduse
                        .Where(sp => sp.IdProdus == request.IdProdus && sp.idCos == request.idCos)
                        .OrderBy(sp => sp.IdSubprodus)
                        .FirstOrDefaultAsync();

                    if (subToRemove == null)
                        return NotFound(new { message = "No subproduct found in the cart for this product." });

                    subToRemove.idCos = null;
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "One subproduct removed from cart." });

                case "remove-all":
                    var subAll = await _context.SubProduse
                        .Where(sp => sp.IdProdus == request.IdProdus && sp.idCos == request.idCos)
                        .ToListAsync();

                    if (!subAll.Any())
                        return NotFound(new { message = "No subproducts found in the cart for this product." });

                    foreach (var sp in subAll)
                    {
                        sp.idCos = null;
                    }
                    await _context.SaveChangesAsync();
                    return Ok(new { message = "All subproducts for the product removed from cart." });

                default:
                    return BadRequest(new { message = "Invalid operation: " + operation });
            }
        }
        [HttpDelete("user/{userId}")]
        public async Task<IActionResult> DeleteCartByUserId(int userId)
        {
            // 1) Verificam daca userul exista
            var user = await _context.Users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // 2) Verificam daca exista un cos asociat
            var cart = await _cosRepository.GetCartByUserId(userId);
            if (cart == null)
            {
                return NotFound(new { message = "Cart not found for user." });
            }

            // 3) Ștergem coșul
            _context.Cosuri.Remove(cart);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Cart deleted successfully." });
        }
        [HttpPost("addSubproduseToCart")]
        public async Task<IActionResult> AddMultipleSubproduseToCart([FromBody] SubprodusAdaugareDTO dto)
        {
            if (dto == null || dto.IdProdus <= 0 || dto.Quantity <= 0 || dto.IdUser <= 0 || dto.idCos <= 0)
                return BadRequest(new { message = "Datele trimise sunt invalide." });

            var produs = await _context.Products.FindAsync(dto.IdProdus);
            if (produs == null)
                return NotFound(new { message = "Produsul nu există." });

            var cos = await _context.Cosuri.FindAsync(dto.idCos);
            if (cos == null)
                return NotFound(new { message = "Coșul nu există." });

            var subproduseDisponibile = await _context.SubProduse
                .Where(sp => sp.IdProdus == dto.IdProdus && sp.idCos == null)
                .Take(dto.Quantity)
                .ToListAsync();

            if (subproduseDisponibile.Count < dto.Quantity)
                return BadRequest(new { message = "Nu există suficiente subproduse disponibile." });

            foreach (var sp in subproduseDisponibile)
            {
                sp.idCos = dto.idCos;
            }

            await _context.SaveChangesAsync();

            return Ok(new { message = $"{dto.Quantity} subproduse adăugate în coș." });
        }

        [HttpGet("cart-content/{userId}")]
        public async Task<IActionResult> GetCartContentByUserId(int userId)
        {
            //Check if user has a cart
            var userCart = await _context.Cosuri
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(c => c.IdUser == userId);

            if (userCart == null)
            {
                return NotFound(new { message = "User does not have a cart." });
            }

            //Retrieve subproducts from that cart
            var cartContent = await (
                from sp in _context.SubProduse
                where sp.idCos == userCart.idCos

                join p in _context.Products on sp.IdProdus equals p.IdProdus
                //left join
                join cat in _context.Categorii on p.IdCategorie equals cat.IdCategorie into catJoin
                from cat in catJoin.DefaultIfEmpty()

                    // (Optional) get latest price from Preturi_Produs
                let lastPrice = _context.Preturi_Produs
                    .Where(pp => pp.IdProdus == p.IdProdus)
                    .OrderByDescending(pp => pp.DataInceput)
                    .Select(pp => (decimal?)pp.Pret)
                    .FirstOrDefault() // can be null

                // (Optional) get image from Imagini
                let imageFile = _context.Imagini
                    .Where(img => img.IdProdus == p.IdProdus)
                    .Select(img => img.Fisier)
                    .FirstOrDefault() // can be null

                // (Optional) get color code from Vopsele
                let colorCode = _context.Vopsele
                    .Where(v => v.IdProdus == p.IdProdus)
                    .Select(v => v.CodCuloare)
                    .FirstOrDefault() 

                group new { sp, p, cat, lastPrice, imageFile, colorCode }
                by new
                {
                    p.IdProdus,
                    p.Nume,
                    p.EsteSpray,
                    Category = (cat != null ? cat.DenumireCategorie : "Necunoscut"),
                    Price = (lastPrice ?? 0), 
                    ColorCuloare = (colorCode ?? "N/A"),
                    Imagine = imageFile
                }
                into g
                select new
                {
                    IdProdus = g.Key.IdProdus,
                    Nume = g.Key.Nume,
                    Pret = g.Key.Price,
                    Categorie = g.Key.Category,
                    EsteSpray = g.Key.EsteSpray,
                    CodCuloare = g.Key.ColorCuloare,
                    Imagine = g.Key.Imagine,
                    Cantitatea = g.Count() 
                }
            ).ToListAsync();

            return Ok(new
            {
                cart = userCart,
                products = cartContent
            });
        }

    }
}
