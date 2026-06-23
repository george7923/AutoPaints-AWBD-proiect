using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public class PersoanaService : IPersoanaRepository
    {
        private readonly ApplicationDbContext _context;

        public PersoanaService(ApplicationDbContext context)
        {
            _context = context;
        }

        // =========================
        // METODE EXISTENTE DIN IPersoanaRepository
        // =========================

        public async Task<IEnumerable<Persoana>> GetAll()
        {
            return await _context.Persoane.ToListAsync();
        }

        public async Task<Persoana?> GetById(int id)
        {
            return await _context.Persoane.FindAsync(id);
        }

        public async Task<Persoana?> GetByEmail(string email)
        {
            return await _context.Persoane
                .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower());
        }

        public async Task CreatePersoana(Persoana persoana)
        {
            await _context.Persoane.AddAsync(persoana);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePersoanaById(int id, Persoana updatedPersoana)
        {
            var persoanaToUpdate = await _context.Persoane
                .FirstOrDefaultAsync(p => p.IdPersoana == id);

            if (persoanaToUpdate != null)
            {
                persoanaToUpdate.Nume = updatedPersoana.Nume ?? persoanaToUpdate.Nume;
                persoanaToUpdate.Prenume = updatedPersoana.Prenume ?? persoanaToUpdate.Prenume;
                persoanaToUpdate.Email = updatedPersoana.Email ?? persoanaToUpdate.Email;
                persoanaToUpdate.tipPersoana = updatedPersoana.tipPersoana ?? persoanaToUpdate.tipPersoana;
                persoanaToUpdate.Telefon = updatedPersoana.Telefon ?? persoanaToUpdate.Telefon;

                _context.Entry(persoanaToUpdate).State = EntityState.Modified;
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeletePersoanaById(int id)
        {
            var persoanaToDelete = await _context.Persoane
                .FirstOrDefaultAsync(p => p.IdPersoana == id);

            if (persoanaToDelete != null)
            {
                _context.Persoane.Remove(persoanaToDelete);
                await _context.SaveChangesAsync();
            }
        }

        // =========================
        // LOGICA MUTATA DIN PersoanaController
        // =========================

        public async Task<IActionResult> GetByIdResponse(int id)
        {
            var persoana = await GetById(id);

            if (persoana == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(persoana);
        }

        public async Task<IActionResult> GetByEmailResponse(string email)
        {
            var persoana = await GetByEmail(email);

            if (persoana == null)
            {
                return new NotFoundResult();
            }

            return new OkObjectResult(persoana);
        }

        public async Task<IActionResult> CreateResponse(Persoana persoana)
        {
            await CreatePersoana(persoana);

            return new CreatedAtActionResult(
                "GetById",
                "Persoana",
                new { id = persoana.IdPersoana },
                persoana
            );
        }

        public async Task<IActionResult> UpdatePersoanaByIdResponse(int id, Persoana updatedPersoana)
        {
            if (updatedPersoana == null)
            {
                return new BadRequestObjectResult(new
                {
                    error = "Invalid Persoana data"
                });
            }

            await UpdatePersoanaById(id, updatedPersoana);

            return new OkObjectResult(new
            {
                message = "Persoana actualizata cu succes"
            });
        }

        public async Task<IActionResult> DeletePersoanaByIdResponse(int id)
        {
            var persoana = await GetById(id);

            if (persoana == null)
            {
                return new NotFoundObjectResult(new
                {
                    message = "Persoana not found"
                });
            }

            await DeletePersoanaById(id);

            return new OkObjectResult(new
            {
                message = "Persoana deleted successfully"
            });
        }

        public IActionResult SendEmailResponse(PersoanaController.EmailRequest request)
        {
            try
            {
                var fromAddress = new MailAddress("kobrageorge792@gmail.com", "AutoPaints Team");
                var toAddress = new MailAddress(request.To);
                const string fromPassword = "oarl ezrd ecti fmuo";  // Gmail app password
                string subject = request.Subject;
                string body = request.Body;

                var smtp = new SmtpClient
                {
                    Host = "smtp.gmail.com",
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(fromAddress.Address, fromPassword)
                };

                using (var message = new MailMessage(fromAddress, toAddress)
                {
                    Subject = subject,
                    Body = body
                })
                {
                    smtp.Send(message);
                }

                return new OkObjectResult(new
                {
                    message = "Email trimis cu succes!"
                });
            }
            catch (Exception ex)
            {
                return new ObjectResult(new
                {
                    error = "Eroare la trimiterea emailului",
                    details = ex.Message
                })
                {
                    StatusCode = 500
                };
            }
        }
    }
}