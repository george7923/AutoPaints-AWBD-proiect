using Microsoft.EntityFrameworkCore;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.Models;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<int?> GetUserIdByUsername(string username)
        {
            var user = await _context.Users
                .Where(u => u.Username == username)
                .Select(u => u.IdUser)
                .FirstOrDefaultAsync();

            return user == 0 ? null : user;
        }
        public async Task<User?> GetByUsername(string username)
        {
            var user = await _context.Users
                .Include(u => u.Persoana) 
                .FirstOrDefaultAsync(u => u.Username == username);

            if (user == null) return null;
            var userAddresses = await _context.Adrese_Useri
                .Where(au => au.IdUser == user.IdUser)
                .Select(au => au.Adrese) 
                .ToListAsync();


            return new User
            {
                IdUser = user.IdUser,
                Username = user.Username,
                Password = user.Password,
                IdPersoana = user.IdPersoana,
                Persoana = user.Persoana,

            };
        }





        public async Task<User?> GetByEmail(string email) =>
            await _context.Users
                .Include(u => u.Persoana)
                .FirstOrDefaultAsync(u => u.Persoana.Email.ToLower() == email.ToLower());

        public async Task CreateUser(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
        }

        public async Task<User?> UpdateUser(User user)
        {

            var existingUser = await _context.Users
                .Include(u => u.Persoana)
                .FirstOrDefaultAsync(u => u.IdUser == user.IdUser);
            if (existingUser == null)
                return null;


            existingUser.Username = user.Username ?? existingUser.Username;
            if (!string.IsNullOrEmpty(user.Password))
            {
                existingUser.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            }


            if (user.Persoana != null)
            {
                existingUser.Persoana.Nume = user.Persoana.Nume ?? existingUser.Persoana.Nume;
                existingUser.Persoana.Prenume = user.Persoana.Prenume ?? existingUser.Persoana.Prenume;
                existingUser.Persoana.Email = user.Persoana.Email ?? existingUser.Persoana.Email;
                existingUser.Persoana.tipPersoana = user.Persoana.tipPersoana ?? existingUser.Persoana.tipPersoana;
                existingUser.Persoana.Telefon = user.Persoana.Telefon ?? existingUser.Persoana.Telefon;
            }

            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task<bool> DeleteUser(int idUser)
        {
            var user = await _context.Users.FindAsync(idUser);
            if (user == null)
                return false;

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<User?> GetById(int id)
        {
            return await _context.Users
                .Include(u => u.Persoana) 
                .FirstOrDefaultAsync(u => u.IdUser == id);
        }

    }
}
