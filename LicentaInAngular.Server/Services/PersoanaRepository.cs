using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public class PersoanaRepository : IPersoanaRepository
    {
        private readonly ApplicationDbContext _context;

        public PersoanaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

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
            var persoanaToUpdate = await _context.Persoane.FirstOrDefaultAsync(p => p.IdPersoana == id);
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
            var persoanaToDelete = await _context.Persoane.FirstOrDefaultAsync(p => p.IdPersoana == id);
            if (persoanaToDelete != null)
            {
                _context.Persoane.Remove(persoanaToDelete);
                await _context.SaveChangesAsync();
            }
        }
    }
}
