using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using LicentaInAngular.Server.DataLayer.DTO;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;
using Stripe;

namespace LicentaInAngular.Server.Repositories
{
    public class ComandaService : IComandaRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly ICosRepository _cosRepository;
        private readonly ISubcomandaRepository _subcomandaRepository;

        public ComandaService(
            ApplicationDbContext context,
            ICosRepository cosRepository,
            ISubcomandaRepository subcomandaRepository)
        {
            _context = context;
            _cosRepository = cosRepository;
            _subcomandaRepository = subcomandaRepository;
            QuestPDF.Settings.License = LicenseType.Community;
        }

        // =========================
        // METODE EXISTENTE DIN IComandaRepository
        // =========================

        public async Task<ComandaDTO?> GetById(int id)
        {
            /*
            return await _context.Comenzi
                .Include(c => c.User)
                .Include(c => c.Adresa)
                .Where(c => c.IdComanda == id)
                .(c => new ComandaDTO
                {
                    IdComanda = c.IdComanda,
                    IdUser = c.IdUser,
                    IdAdresa = c.IdAdresa,
                    ETA = c.ETA,

                    // Detalii utilizator
                    Username = c.User.Username,

                    // Detalii adresa
                    Tara = c.Adresa.Tara,
                    Judet = c.Adresa.Judet,
                    Localitate = c.Adresa.Localitate,
                    Strada = c.Adresa.Strada,
                    Bloc = c.Adresa.Bloc,
                    Scara = c.Adresa.Scara,
                    Etaj = c.Adresa.Etaj,
                    Apartament = c.Adresa.Apartament
                })
                .FirstOrDefaultAsync();
            */
            return null;
        }

        public async Task<IEnumerable<ComandaDTO>> GetAllOrders()
        {
            /*
            return await _context.Comenzi
                .Include(c => c.User)
                .Include(c => c.Adresa)
                .(c => new ComandaDTO
                {
                    IdComanda = c.IdComanda,
                    IdUser = c.IdUser,
                    IdAdresa = c.IdAdresa,
                    ETA = c.ETA,

                    // Detalii utilizator
                    Username = c.User.Username,

                    // Detalii adresa
                    Tara = c.Adresa.Tara,
                    Judet = c.Adresa.Judet,
                    Localitate = c.Adresa.Localitate,
                    Strada = c.Adresa.Strada,
                    Bloc = c.Adresa.Bloc,
                    Scara = c.Adresa.Scara,
                    Etaj = c.Adresa.Etaj,
                    Apartament = c.Adresa.Apartament
                })
                .ToListAsync();
            */
            return null;
        }

        public async Task CreateComanda(Comanda comanda)
        {
            await _context.Comenzi.AddAsync(comanda);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateComanda(Comanda comanda)
        {
            _context.Entry(comanda).State = EntityState.Modified;
            await _context.SaveChangesAsync();
        }

        public async Task DeleteComanda(int id)
        {
            var comanda = await _context.Comenzi.FindAsync(id);

            if (comanda != null)
            {
                _context.Comenzi.Remove(comanda);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<ComandaDTO>> GetComenziByUserId(int userId)
        {
            /*
            return await _context.Comenzi
                .Include(c => c.User)
                .Include(c => c.Adresa)
                .Where(c => c.IdUser == userId)
                .(c => new ComandaDTO
                {
                    IdComanda = c.IdComanda,
                    IdUser = c.IdUser,
                    IdAdresa = c.IdAdresa,
                    ETA = c.ETA,

                    // Detalii utilizator
                    Username = c.User.Username,

                    // Detalii adresa
                    Tara = c.Adresa.Tara,
                    Judet = c.Adresa.Judet,
                    Localitate = c.Adresa.Localitate,
                    Strada = c.Adresa.Strada,
                    Bloc = c.Adresa.Bloc,
                    Scara = c.Adresa.Scara,
                    Etaj = c.Adresa.Etaj,
                    Apartament = c.Adresa.Apartament
                })
                .ToListAsync();
            */
            return null;
        }

        public async Task<Comanda> SubmitComanda(ComandaSubmitDTO comandaDto)
        {
            return null;
        }

        public async Task<bool> MarcheazaComandaCaLivrata(int idComanda)
        {
            var comanda = await _context.Comenzi.FindAsync(idComanda);

            if (comanda == null)
                return false;

            comanda.IsPlaced = true;
            comanda.ETA = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> EmitereComanda(int userId, int idAdresa, int? idCard)
        {
            var cos = await _context.Cosuri
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.IdUser == userId);

            if (cos == null)
                throw new Exception("Coșul nu a fost găsit.");

            var subproduse = await _context.SubProduse
                .Where(sp => sp.idCos == cos.idCos && sp.Valabil)
                .Include(sp => sp.Produs)
                .ToListAsync();

            if (!subproduse.Any())
                throw new Exception("Coșul este gol.");

            double pretTotal = 0;

            foreach (var sp in subproduse)
            {
                var pret = await _context.Preturi_Produs
                    .Where(p => p.IdProdus == sp.IdProdus)
                    .OrderByDescending(p => p.DataInceput)
                    .Select(p => p.Pret)
                    .FirstOrDefaultAsync();

                pretTotal += (double)pret;
            }

            var comanda = new Comanda
            {
                IdUser = userId,
                IdAdresa = idAdresa,
                IdCard_CC = idCard,
                ETA = DateTime.UtcNow,
                IsPlaced = false,
                PretTotal = pretTotal
            };

            await _context.Comenzi.AddAsync(comanda);
            await _context.SaveChangesAsync();

            var grupate = subproduse.GroupBy(sp => sp.IdProdus);

            foreach (var grup in grupate)
            {
                var subcomanda = new Subcomanda
                {
                    IdProdus = grup.Key,
                    IdComanda = comanda.IdComanda,
                    TotalSubproduse = grup.Count()
                };

                await _context.Subcomenzi.AddAsync(subcomanda);

                foreach (var sp in grup)
                {
                    sp.idCos = null;
                }
            }

            await _context.SaveChangesAsync();

            return comanda.IdComanda;
        }

        public async Task<IEnumerable<ComandaCuDetaliiDTO>> GetToateComenzileAleUtilizatorului(int userId)
        {
            var comenzi = await _context.Comenzi
                .Where(c => c.IdUser == userId)
                .Include(c => c.Adresa)
                    .ThenInclude(a => a.Strazi)
                        .ThenInclude(s => s.Localitati)
                            .ThenInclude(l => l.Judete)
                                .ThenInclude(j => j.Tari)
                .Select(c => new ComandaCuDetaliiDTO
                {
                    IdComanda = c.IdComanda,
                    ETA = c.ETA,
                    IsPlaced = c.IsPlaced,
                    PretTotal = c.PretTotal,

                    Tara = c.Adresa.Strazi.Localitati.Judete.Tari.DenumireTara,
                    Judet = c.Adresa.Strazi.Localitati.Judete.DenumireJudet,
                    Localitate = c.Adresa.Strazi.Localitati.DenumireLocalitate,
                    Strada = c.Adresa.Strazi.DenumireStrada,
                    Nr = c.Adresa.Strazi.Nr,
                    Bloc = c.Adresa.Bloc,
                    Scara = c.Adresa.Scara,
                    Etaj = c.Adresa.Etaj,
                    Apartament = c.Adresa.Apartament,

                    Produse = _context.Subcomenzi
                        .Where(s => s.IdComanda == c.IdComanda)
                        .Select(s => new ProdusComandaDTO
                        {
                            NumeProdus = s.Produs.Nume,
                            Cantitate = s.TotalSubproduse
                        }).ToList()
                })
                .ToListAsync();

            return comenzi;
        }

        // =========================
        // LOGICA MUTATA DIN ComandaController
        // =========================

        public async Task<IActionResult> GetComandaByIdResponse(int id)
        {
            var comanda = await GetById(id);

            if (comanda == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(comanda);
        }

        public async Task<IActionResult> GetAllComenziResponse()
        {
            var comenzi = await GetAllOrders();
            return new OkObjectResult(comenzi);
        }

        public async Task<IActionResult> GetComenziByUserIdResponse(int userId)
        {
            var comenzi = await GetComenziByUserId(userId);

            if (comenzi == null || !comenzi.Any())
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(comenzi);
        }

        public async Task<IActionResult> SubmitComandaWithPaymentResponse(ComandaPaymentDTO dto)
        {
            if (dto == null || dto.Payment == null || dto.Comanda == null)
            {
                return new BadRequestObjectResult("Payment or Comanda data is missing.");
            }

            StripeConfiguration.ApiKey = "sk_test_YourSecretKeyHere";

            try
            {
                var paymentOptions = new PaymentIntentCreateOptions
                {
                    Amount = dto.Payment.AmountInBani,
                    Currency = "ron",
                    PaymentMethod = dto.Payment.PaymentMethodId,
                    Description = dto.Payment.Description,
                    Confirm = true
                };

                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = paymentIntentService.Create(paymentOptions);

                if (paymentIntent.Status != "succeeded")
                {
                    return new BadRequestObjectResult(new
                    {
                        message = "Payment not completed",
                        status = paymentIntent.Status
                    });
                }

                var newComanda = new Comanda
                {
                    IdUser = dto.Comanda.IdUser,
                    IdAdresa = dto.Comanda.IdAdresa,
                    PretTotal = (double)dto.Comanda.PretTotal,
                    ETA = DateTime.UtcNow.AddDays(3)
                };

                await CreateComanda(newComanda);

                var cosProducts = await _cosRepository.GetProductsByUserId(dto.Comanda.IdUser);

                foreach (var product in cosProducts)
                {
                    var newSubcomanda = new Subcomanda
                    {
                        IdProdus = product.IdProdus,
                        TotalSubproduse = product.Quantity,
                        IdComanda = newComanda.IdComanda
                    };

                    await _subcomandaRepository.AddSubcomanda(newSubcomanda);
                }

                await _cosRepository.ClearCart(dto.Comanda.IdUser);

                return new CreatedAtActionResult(
                    "GetComandaById",
                    "Comanda",
                    new { id = newComanda.IdComanda },
                    new
                    {
                        message = "Payment successful, Comanda created.",
                        comanda = newComanda,
                        paymentIntentId = paymentIntent.Id
                    }
                );
            }
            catch (StripeException ex)
            {
                return new BadRequestObjectResult(new
                {
                    error = ex.StripeError?.Message ?? ex.Message
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "An error occurred while processing the payment",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> UpdateComandaResponse(int id, Comanda comanda)
        {
            if (id != comanda.IdComanda)
            {
                return new BadRequestObjectResult("Comanda ID mismatch.");
            }

            var existingComanda = await GetById(id);

            if (existingComanda == null)
            {
                return new NotFoundResult();
            }

            await UpdateComanda(comanda);

            return new NoContentResult();
        }

        public async Task<IActionResult> DeleteComandaResponse(int id)
        {
            var existingComanda = await GetById(id);

            if (existingComanda == null)
            {
                return new NotFoundResult();
            }

            await DeleteComanda(id);

            return new NoContentResult();
        }

        public async Task<IActionResult> EmitereComandaResponse(ComandaEmitereDTO dto, bool isModelValid)
        {
            if (!isModelValid)
            {
                return new BadRequestObjectResult("Datele trimise nu sunt valide.");
            }

            try
            {
                var idComanda = await EmitereComanda(dto.IdUser, dto.IdAdresa, dto.IdCard);

                _context.ChangeTracker.Clear();

                await MarcheazaSubproduseNevalabile(idComanda);

                return new OkObjectResult(new
                {
                    idComanda = idComanda,
                    message = "Comanda a fost înregistrată, subprodusele setate ca nevalabile."
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "Eroare la emiterea comenzii",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> MarcheazaCaLivrataSauNelivrataResponse(int id, LivrareDto dto)
        {
            try
            {
                var comanda = await _context.Comenzi.FindAsync(id);

                if (comanda == null)
                {
                    return new NotFoundObjectResult(new
                    {
                        message = "Comanda nu a fost găsită."
                    });
                }

                comanda.IsPlaced = dto.Livrata;

                await _context.SaveChangesAsync();

                return new OkObjectResult(new
                {
                    message = $"Comanda a fost marcată ca livrată: {dto.Livrata}"
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "Eroare la modificarea livrării.",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> GetToateComenzileAleUtilizatoruluiResponse(int userId)
        {
            var comenzi = await GetToateComenzileAleUtilizatorului(userId);
            return new OkObjectResult(comenzi);
        }

        private async Task MarcheazaSubproduseNevalabile(int idComanda)
        {
            var subcomenzi = await _context.Subcomenzi
                .Where(sc => sc.IdComanda == idComanda)
                .ToListAsync();

            foreach (var subcomanda in subcomenzi)
            {
                var subproduse = await _context.SubProduse
                    .Where(sp => sp.IdProdus == subcomanda.IdProdus && sp.Valabil == true && sp.idCos == null)
                    .Take(subcomanda.TotalSubproduse)
                    .ToListAsync();

                foreach (var sp in subproduse)
                {
                    sp.Valabil = false;
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IActionResult> EmitereComandaCashResponse(ComandaEmitereDTO dto, bool isModelValid)
        {
            if (!isModelValid)
            {
                return new BadRequestObjectResult("Datele trimise nu sunt valide.");
            }

            try
            {
                var idComanda = await EmitereComanda(dto.IdUser, dto.IdAdresa, null);

                await MarcheazaSubproduseNevalabile(idComanda);

                return new OkObjectResult(new
                {
                    idComanda = idComanda,
                    message = "Comanda (cash) a fost înregistrată și subprodusele au fost marcate nevalabile."
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "Eroare la emiterea comenzii cu plată la domiciliu",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }

        public async Task<IActionResult> GetComenziScurtByUserResponse(int userId)
        {
            var comenzi = await _context.Comenzi
                .Where(c => c.IdUser == userId)
                .Select(c => new
                {
                    IdComanda = c.IdComanda,
                    PretTotal = c.PretTotal,
                    Livrata = c.IsPlaced,
                    Plata = c.IdCard_CC == null ? "Plata la domiciliu" : "Plata cu cardul"
                })
                .ToListAsync();

            return new OkObjectResult(comenzi);
        }

        public async Task<IActionResult> GenerareBonFiscalResponse(int idComanda)
        {
            try
            {
                QuestPDF.Settings.License = LicenseType.Community;

                var comanda = await _context.Comenzi
                    .Include(c => c.Adresa)
                        .ThenInclude(a => a.Strazi)
                            .ThenInclude(s => s.Localitati)
                                .ThenInclude(l => l.Judete)
                                    .ThenInclude(j => j.Tari)
                    .Include(c => c.User)
                        .ThenInclude(u => u.Persoana)
                    .Include(c => c.Card_CC)
                    .FirstOrDefaultAsync(c => c.IdComanda == idComanda);

                if (comanda == null)
                {
                    return new NotFoundObjectResult("Comanda nu a fost găsită");
                }

                var produse = await _context.Subcomenzi
                    .Where(s => s.IdComanda == idComanda)
                    .Include(s => s.Produs)
                    .Select(s => new
                    {
                        Nume = s.Produs.Nume,
                        Cantitate = s.TotalSubproduse,
                        PretUnitar = _context.Preturi_Produs
                            .Where(p => p.IdProdus == s.IdProdus)
                            .OrderByDescending(p => p.DataInceput)
                            .Select(p => p.Pret)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                double totalComanda = produse.Sum(p => p.Cantitate * (double)p.PretUnitar);

                var stream = new MemoryStream();

                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Margin(30);

                        page.Content().Column(col =>
                        {
                            col.Item().Text("BON FISCAL").FontSize(20).Bold().AlignCenter();

                            col.Item().Text("Distribuitor: AutoPaints").Bold();
                            col.Item().Text("Email: AutoPaints@gmail.com");
                            col.Item().Text("Telefon: 0725454545");

                            col.Item().PaddingVertical(10).LineHorizontal(1);

                            var persoana = comanda.User.Persoana;

                            col.Item().Text("Client:").Bold();
                            col.Item().Text($"{persoana.Nume} {persoana.Prenume}");
                            col.Item().Text($"Email: {persoana.Email}");
                            col.Item().Text($"Telefon: {persoana.Telefon}");

                            col.Item().PaddingVertical(10).LineHorizontal(1);

                            col.Item().Text("Produse comandate:").Bold();

                            foreach (var p in produse)
                            {
                                var totalProdus = p.Cantitate * (double)p.PretUnitar;
                                var tva = totalProdus * 0.19;

                                col.Item().Text($"- {p.Nume} x{p.Cantitate} = {totalProdus:F2} lei (TVA 19%: {tva:F2} lei)");
                            }

                            col.Item().PaddingVertical(10).LineHorizontal(1);

                            col.Item().Text($"Total comandă: {comanda.PretTotal:F2} lei").Bold();

                            col.Item().PaddingVertical(10).LineHorizontal(1);

                            col.Item().Text("Modalitate de plată:").Bold();
                            col.Item().Text(comanda.IdCard_CC != 0 ? "Plată cu cardul" : "Plată la livrare");

                            col.Item().Text("Plată efectuată:").Bold();
                            col.Item().Text((comanda.IdCard_CC != 0 || comanda.IsPlaced) ? "DA" : "NU");

                            col.Item().Text("Comandă livrată:").Bold();
                            col.Item().Text(comanda.IsPlaced ? "DA" : "NU");

                            col.Item().PaddingTop(10).Text($"Data: {DateTime.Now:dd/MM/yyyy}");
                        });
                    });
                }).GeneratePdf(stream);

                stream.Position = 0;

                return new FileStreamResult(stream, "application/pdf")
                {
                    FileDownloadName = "bon-fiscal.pdf"
                };
            }
            catch (Exception ex)
            {
                return new ObjectResult($"Eroare internă la generarea PDF-ului: {ex.Message}")
                {
                    StatusCode = 500
                };
            }
        }
    }
}