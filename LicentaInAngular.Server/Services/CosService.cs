using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;

namespace LicentaInAngular.Server.Repositories
{
    public class CosService : ICosRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly string _errorInvalidRequest = "Invalid request data.";

        public CosService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // METODE EXISTENTE DIN ICosRepository
        // =========================

        // 1. Get the cart by UserId
        public async Task<Cos?> GetCartByUserId(int userId)
        {
            return await _context.Cosuri
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.IdUser == userId);
        }

        // 2. Get products + quantity in the cart
        public async Task<List<CartProductDTO>> GetProductsByUserId(int userId)
        {
            // Find the user's cart
            var cart = await GetCartByUserId(userId);

            if (cart == null)
                return new List<CartProductDTO>();

            // Group subproducts in the cart by product
            var subGroups = await _context.SubProduse
                .Where(sp => sp.idCos == cart.idCos)
                .GroupBy(sp => sp.IdProdus)
                .Select(g => new CartProductDTO
                {
                    IdProdus = g.Key,
                    Quantity = g.Count()
                })
                .ToListAsync();

            return subGroups;
        }

        // 3. Clear all subproducts from the user's cart
        public async Task ClearCart(int userId)
        {
            var cart = await GetCartByUserId(userId);

            if (cart == null)
                return;

            var subproduse = _context.SubProduse
                .Where(sp => sp.idCos == cart.idCos);

            foreach (var sp in subproduse)
            {
                sp.idCos = null; // Unassign from cart
            }

            await _context.SaveChangesAsync();
        }

        public async Task<Cos?> GetById(int id)
        {
            return await _context.Cosuri
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.idCos == id);
        }

        public async Task<IEnumerable<Cos>> GetAllCarts()
        {
            return await _context.Cosuri
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task CreateCart(Cos cos)
        {
            await _context.Cosuri.AddAsync(cos);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateCart(Cos cos)
        {
            _context.Entry(cos).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteCart(int id)
        {
            var cos = await _context.Cosuri.FindAsync(id);

            if (cos != null)
            {
                _context.Cosuri.Remove(cos);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteOldCarts(DateTime thresholdDate)
        {
            var oldCarts = await _context.Cosuri
                .Where(c => c.DataCreare <= thresholdDate)
                .ToListAsync();

            if (oldCarts.Any())
            {
                _context.Cosuri.RemoveRange(oldCarts);
                await _context.SaveChangesAsync();
            }
        }

        // =========================
        // LOGICA MUTATA DIN CosController
        // =========================

        public async Task<IActionResult> GetCartByUserIdResponse(int userId)
        {
            var cart = await GetCartByUserId(userId);

            if (cart == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Cart not found for the given user."
                });
            }

            return new OkObjectResult(cart);
        }

        public async Task<IActionResult> GetCartByIdResponse(int id)
        {
            var cart = await _context.Cosuri.FindAsync(id);

            if (cart == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(cart);
        }

        public async Task<IActionResult> CheckOrCreateCartResponse(int userId)
        {
            if (userId <= 0)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Invalid user ID."
                });
            }

            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Utilizator inexistent.Nu se poate crea un coș."
                });
            }

            var existingCart = await GetCartByUserId(userId);

            if (existingCart != null)
            {
                return new ConflictObjectResult(new
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

                return new CreatedAtActionResult(
                    "GetCartByUserId",
                    "Cos",
                    new { id = newCart.idCos },
                    newCart
                );
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    message = "Eroare internă la creare coș.",
                    detail = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> GetCartDetailsByUserResponse(int userId)
        {
            var cart = await GetCartByUserId(userId);

            if (cart == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Cart not found for the given user."
                });
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

            return new OkObjectResult(new
            {
                Cart = cart,
                Items = items
            });
        }

        public async Task<IActionResult> AddOneSubprodusToCartResponse(SubprodusUpdateDTO request)
        {
            if (request == null || request.IdProdus <= 0 || request.idCos <= 0)
            {
                return new BadRequestObjectResult(new
                {
                    message = _errorInvalidRequest
                });
            }

            return await UpdateCartSubprodus(request, "add-one");
        }

        public async Task<IActionResult> RemoveOneSubprodusFromCartResponse(SubprodusUpdateDTO request)
        {
            if (request == null || request.IdProdus <= 0 || request.idCos <= 0)
            {
                return new BadRequestObjectResult(new
                {
                    message = _errorInvalidRequest
                });
            }

            return await UpdateCartSubprodus(request, "remove-one");
        }

        public async Task<IActionResult> RemoveAllSubproduseFromCartResponse(SubprodusUpdateDTO request)
        {
            if (request == null || request.IdProdus <= 0 || request.idCos <= 0)
            {
                return new BadRequestObjectResult(new
                {
                    message = _errorInvalidRequest
                });
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
                    {
                        return new NotFoundObjectResult(new
                        {
                            message = "No available subproduct for this product."
                        });
                    }

                    // Asignam subprodusul la cos
                    subToAdd.idCos = request.idCos;
                    await _context.SaveChangesAsync();

                    return new OkObjectResult(new
                    {
                        message = "One subproduct added to cart."
                    });

                case "remove-one":
                    var subToRemove = await _context.SubProduse
                        .Where(sp => sp.IdProdus == request.IdProdus && sp.idCos == request.idCos)
                        .OrderBy(sp => sp.IdSubprodus)
                        .FirstOrDefaultAsync();

                    if (subToRemove == null)
                    {
                        return new NotFoundObjectResult(new
                        {
                            message = "No subproduct found in the cart for this product."
                        });
                    }

                    subToRemove.idCos = null;
                    await _context.SaveChangesAsync();

                    return new OkObjectResult(new
                    {
                        message = "One subproduct removed from cart."
                    });

                case "remove-all":
                    var subAll = await _context.SubProduse
                        .Where(sp => sp.IdProdus == request.IdProdus && sp.idCos == request.idCos)
                        .ToListAsync();

                    if (!subAll.Any())
                    {
                        return new NotFoundObjectResult(new
                        {
                            message = "No subproducts found in the cart for this product."
                        });
                    }

                    foreach (var sp in subAll)
                    {
                        sp.idCos = null;
                    }

                    await _context.SaveChangesAsync();

                    return new OkObjectResult(new
                    {
                        message = "All subproducts for the product removed from cart."
                    });

                default:
                    return new BadRequestObjectResult(new
                    {
                        message = "Invalid operation: " + operation
                    });
            }
        }

        public async Task<IActionResult> DeleteCartByUserIdResponse(int userId)
        {
            // 1) Verificam daca userul exista
            var user = await _context.Users.FindAsync(userId);

            if (user == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "User not found."
                });
            }

            // 2) Verificam daca exista un cos asociat
            var cart = await GetCartByUserId(userId);

            if (cart == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Cart not found for user."
                });
            }

            // 3) Stergem cosul
            _context.Cosuri.Remove(cart);
            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = "Cart deleted successfully."
            });
        }

        public async Task<IActionResult> AddMultipleSubproduseToCartResponse(SubprodusAdaugareDTO dto)
        {
            if (dto == null || dto.IdProdus <= 0 || dto.Quantity <= 0 || dto.IdUser <= 0 || dto.idCos <= 0)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Datele trimise sunt invalide."
                });
            }

            var produs = await _context.Products.FindAsync(dto.IdProdus);

            if (produs == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Produsul nu există."
                });
            }

            var cos = await _context.Cosuri.FindAsync(dto.idCos);

            if (cos == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Coșul nu există."
                });
            }

            var subproduseDisponibile = await _context.SubProduse
                .Where(sp => sp.IdProdus == dto.IdProdus && sp.idCos == null)
                .Take(dto.Quantity)
                .ToListAsync();

            if (subproduseDisponibile.Count < dto.Quantity)
            {
                return new BadRequestObjectResult(new
                {
                    message = "Nu există suficiente subproduse disponibile."
                });
            }

            foreach (var sp in subproduseDisponibile)
            {
                sp.idCos = dto.idCos;
            }

            await _context.SaveChangesAsync();

            return new OkObjectResult(new
            {
                message = $"{dto.Quantity} subproduse adăugate în coș."
            });
        }

        public async Task<IActionResult> GetCartContentByUserIdResponse(int userId)
        {
            // Check if user has a cart
            var userCart = await _context.Cosuri
                                         .AsNoTracking()
                                         .FirstOrDefaultAsync(c => c.IdUser == userId);

            if (userCart == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "User does not have a cart."
                });
            }

            // Retrieve subproducts from that cart
            var cartContent = await (
                from sp in _context.SubProduse
                where sp.idCos == userCart.idCos

                join p in _context.Products on sp.IdProdus equals p.IdProdus

                // left join
                join cat in _context.Categorii on p.IdCategorie equals cat.IdCategorie into catJoin
                from cat in catJoin.DefaultIfEmpty()

                    // (Optional) get latest price from Preturi_Produs
                let lastPrice = _context.Preturi_Produs
                    .Where(pp => pp.IdProdus == p.IdProdus)
                    .OrderByDescending(pp => pp.DataInceput)
                    .Select(pp => (decimal?)pp.Pret)
                    .FirstOrDefault()

                // (Optional) get image from Imagini
                let imageFile = _context.Imagini
                    .Where(img => img.IdProdus == p.IdProdus)
                    .Select(img => img.Fisier)
                    .FirstOrDefault()

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

            return new OkObjectResult(new
            {
                cart = userCart,
                products = cartContent
            });
        }
    }
}