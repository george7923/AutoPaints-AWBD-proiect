using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Stripe;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tensorflow.Contexts;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.Extensions.Logging;
using System.Diagnostics;


namespace LicentaInAngular.Server.Controllers
{
    [Route("api/comanda")]
    [ApiController]
    public class ComandaController : ControllerBase
    {
        private readonly ILogger<ComandaController> _logger;
        private readonly IComandaRepository _comandaRepository;
        private readonly ICosRepository _cosRepository;
        private readonly ISubcomandaRepository _subcomandaRepository;
        private readonly ApplicationDbContext _context;

        public ComandaController(IComandaRepository comandaRepository, ICosRepository cosRepository, ISubcomandaRepository subcomandaRepository, ApplicationDbContext context, ILogger<ComandaController> logger)
        {
            _comandaRepository = comandaRepository;
            _cosRepository = cosRepository;
            _subcomandaRepository = subcomandaRepository;
            _context = context;
            QuestPDF.Settings.License = LicenseType.Community;
            _logger = logger;
        }


        [HttpGet("{id}")]
        public async Task<ActionResult<ComandaDTO>> GetComandaById(int id)
        {
            var comanda = await _comandaRepository.GetById(id);
            if (comanda == null)
            {
                return NotFound();
            }
            return Ok(comanda);
        }


        [HttpGet]
        public async Task<ActionResult<IEnumerable<ComandaDTO>>> GetAllComenzi()
        {
            var comenzi = await _comandaRepository.GetAllOrders();
            return Ok(comenzi);
        }


        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<ComandaDTO>>> GetComenziByUserId(int userId)
        {
            var comenzi = await _comandaRepository.GetComenziByUserId(userId);
            if (comenzi == null || !comenzi.Any())
            {
                return NotFound();
            }
            return Ok(comenzi);
        }


        [HttpPost("submit-with-payment")]
        public async Task<IActionResult> SubmitComandaWithPayment([FromBody] ComandaPaymentDTO dto)
        {
            if (dto == null || dto.Payment == null || dto.Comanda == null)
            {
                return BadRequest("Payment or Comanda data is missing.");
            }

            StripeConfiguration.ApiKey = "sk_test_YourSecretKeyHere"; 

            try
            {
                // 1) Process Stripe Payment
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
                    return BadRequest(new { message = "Payment not completed", status = paymentIntent.Status });
                }

                // 2) Create Comanda (Order)
                var newComanda = new Comanda
                {
                    IdUser = dto.Comanda.IdUser,
                    IdAdresa = dto.Comanda.IdAdresa,
                    PretTotal = (double)dto.Comanda.PretTotal, 
                    ETA = DateTime.UtcNow.AddDays(3) 
                };


                await _comandaRepository.CreateComanda(newComanda);


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

                return CreatedAtAction(nameof(GetComandaById), new { id = newComanda.IdComanda }, new
                {
                    message = "Payment successful, Comanda created.",
                    comanda = newComanda,
                    paymentIntentId = paymentIntent.Id
                });
            }
            catch (StripeException ex)
            {
                return BadRequest(new { error = ex.StripeError?.Message ?? ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while processing the payment", details = ex.Message });
            }
        }


        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateComanda(int id, [FromBody] Comanda comanda)
        {
            if (id != comanda.IdComanda)
            {
                return BadRequest("Comanda ID mismatch.");
            }

            var existingComanda = await _comandaRepository.GetById(id);
            if (existingComanda == null)
            {
                return NotFound();
            }

            await _comandaRepository.UpdateComanda(comanda);
            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteComanda(int id)
        {
            var existingComanda = await _comandaRepository.GetById(id);
            if (existingComanda == null)
            {
                return NotFound();
            }

            await _comandaRepository.DeleteComanda(id);
            return NoContent();
        }
        [HttpPost("emitere")]
        public async Task<IActionResult> EmitereComanda([FromBody] ComandaEmitereDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datele trimise nu sunt valide.");

            try
            {
                var idComanda = await _comandaRepository.EmitereComanda(dto.IdUser, dto.IdAdresa, dto.IdCard);

                _context.ChangeTracker.Clear(); 
                await MarcheazaSubproduseNevalabile(idComanda);

                return Ok(new
                {
                    idComanda = idComanda,
                    message = "Comanda a fost înregistrată, subprodusele setate ca nevalabile."
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "Eroare la emiterea comenzii",
                    details = ex.Message
                });
            }
        }








        [HttpPut("livrare/{id}")]
        public async Task<IActionResult> MarcheazaCaLivrataSauNelivrata(int id, [FromBody] LivrareDto dto)
        {
            try
            {
                var comanda = await _context.Comenzi.FindAsync(id);
                if (comanda == null)
                    return NotFound(new { message = "Comanda nu a fost găsită." });

                comanda.IsPlaced = dto.Livrata; // POATE FI TRUE SAU FALSE

                await _context.SaveChangesAsync();
                return Ok(new { message = $"Comanda a fost marcată ca livrată: {dto.Livrata}" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Eroare la modificarea livrării.", details = ex.Message });
            }
        }

        [HttpGet("afisare/{userId}")]
        public async Task<IActionResult> GetToateComenzileAleUtilizatorului(int userId)
        {
            var comenzi = await _comandaRepository.GetToateComenzileAleUtilizatorului(userId);
            return Ok(comenzi);
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

        // [HttpPost("emitere-cash")]
        [HttpPost("emitere-cash")]
        public async Task<IActionResult> EmitereComandaCash([FromBody] ComandaEmitereDTO dto)
        {
            if (!ModelState.IsValid)
                return BadRequest("Datele trimise nu sunt valide.");

            try
            {
                var idComanda = await _comandaRepository.EmitereComanda(dto.IdUser, dto.IdAdresa, null); // fără card
                await MarcheazaSubproduseNevalabile(idComanda);

                return Ok(new { idComanda = idComanda, message = "Comanda (cash) a fost înregistrată și subprodusele au fost marcate nevalabile." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "Eroare la emiterea comenzii cu plată la domiciliu", details = ex.Message });
            }
        }

        [HttpGet("scurt/user/{userId}")]
        public async Task<IActionResult> GetComenziScurtByUser(int userId)
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

            return Ok(comenzi);
        }
        [HttpGet("generare-pdf/{idComanda}")]

        public async Task<IActionResult> GenerareBonFiscal(int idComanda)
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
                    return NotFound("Comanda nu a fost găsită");

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

                            // Distribuitor
                            col.Item().Text("Distribuitor: AutoPaints").Bold();
                            col.Item().Text("Email: AutoPaints@gmail.com");
                            col.Item().Text("Telefon: 0725454545");

                            col.Item().PaddingVertical(10).LineHorizontal(1);

                            // Client
                            var persoana = comanda.User.Persoana;
                            col.Item().Text("Client:").Bold();
                            col.Item().Text($"{persoana.Nume} {persoana.Prenume}");
                            col.Item().Text($"Email: {persoana.Email}");
                            col.Item().Text($"Telefon: {persoana.Telefon}");

                            col.Item().PaddingVertical(10).LineHorizontal(1);

                            // Produse
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

                            // Modalitate plata
                            col.Item().Text("Modalitate de plată:").Bold();
                            col.Item().Text(comanda.IdCard_CC != 0 ? "Plată cu cardul" : "Plată la livrare");

                            // Plata efectuata
                            col.Item().Text("Plată efectuată:").Bold();
                            col.Item().Text((comanda.IdCard_CC != 0 || comanda.IsPlaced) ? "DA" : "NU");

                            // Stare comanda
                            col.Item().Text("Comandă livrată:").Bold();
                            col.Item().Text(comanda.IsPlaced ? "DA" : "NU");

                            col.Item().PaddingTop(10).Text($"Data: {DateTime.Now:dd/MM/yyyy}");
                        });
                    });
                }).GeneratePdf(stream);

                stream.Position = 0;
                return File(stream, "application/pdf", "bon-fiscal.pdf");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Eroare internă la generarea PDF-ului: {ex.Message}");
            }
        }







    }
    public class LivrareDto
    {
        public bool Livrata { get; set; }
    }
}
public class ProdusComandaBonFiscalDTO
{
    public string NumeProdus { get; set; }
    public int Cantitate { get; set; }
    public double PretUnitar { get; set; }
}
public class BonFiscalDTO
{
    public int IdComanda { get; set; }
    public DateTime ETA { get; set; }
    public double PretTotal { get; set; }
    public bool IsPlaced { get; set; }

    public string Tara { get; set; }
    public string Judet { get; set; }
    public string Localitate { get; set; }
    public string Strada { get; set; }
    public int Nr { get; set; }
    public string? Bloc { get; set; }
    public string? Scara { get; set; }
    public string? Etaj { get; set; }
    public string? Apartament { get; set; }

    public List<ProdusComandaBonFiscalDTO> Produse { get; set; } = new();
}
