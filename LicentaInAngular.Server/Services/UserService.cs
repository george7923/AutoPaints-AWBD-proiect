using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.Data;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace LicentaInAngular.Server.Repositories
{
    public class UserService : IUserRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<UserService> _logger;

        public UserService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<UserService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        // =========================
        // METODE EXISTENTE DIN IUserRepository
        // =========================

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
                Persoana = user.Persoana
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

        // =========================
        // LOGICA MUTATA 1 LA 1 DIN AccountController
        // =========================

        public async Task<IActionResult> GetUserIdByUsernameResponse(string username)
        {
            try
            {
                var userId = await GetUserIdByUsername(username);

                if (userId == null)
                {
                    // Error id 404 - Not Found, deoarece utilizatorul nu exista in baza de date.
                    return NotFoundError("User not found");
                }

                return new OkObjectResult(new { IdUser = userId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the user id.");

                // Error id 500 - Internal Server Error, deoarece a aparut o eroare neasteptata la citirea id-ului.
                return InternalServerError("An error occurred while processing your request.");
            }
        }

        public async Task<IActionResult> GetAllUsersResponse()
        {
            try
            {
                // Fara repository, direct pe ApplicationDbContext
                var users = await _context.Users
                    .Select(u => new { IdUser = u.IdUser, Username = u.Username })
                    .ToListAsync();

                return new OkObjectResult(users);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching all users.");

                // Error id 500 - Internal Server Error, deoarece lista de utilizatori nu a putut fi incarcata.
                return InternalServerError("An error occurred while loading the users.");
            }
        }

        public async Task<IActionResult> GetUserByUsernameResponse(string username)
        {
            try
            {
                var user = await GetByUsername(username);

                if (user == null)
                {
                    // Error id 404 - Not Found, deoarece utilizatorul cautat nu exista.
                    return NotFoundError("User not found");
                }

                return new OkObjectResult(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the user details.");

                // Error id 500 - Internal Server Error, deoarece detaliile utilizatorului nu au putut fi incarcate.
                return InternalServerError("An error occurred while processing your request.");
            }
        }

        public async Task<IActionResult> RegisterResponse(RegisterUserDto registerDto)
        {
            if (registerDto == null || registerDto.Persoana == null)
            {
                // Error id 400 - Bad Request, deoarece lipsesc datele de user sau datele personale.
                return BadRequestError("Invalid request. Missing user or personal data.");
            }

            try
            {
                if (string.IsNullOrEmpty(registerDto.Persoana.Email) ||
                    !Regex.IsMatch(registerDto.Persoana.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    // Error id 400 - Bad Request, deoarece email-ul are format invalid.
                    return BadRequestError("Invalid email format.");
                }

                if (await GetByEmail(registerDto.Persoana.Email) != null)
                {
                    // Error id 400 - Bad Request, deoarece logica initiala trateaza email-ul duplicat ca request invalid.
                    return BadRequestError("Email already exists.");
                }

                if (await GetByUsername(registerDto.Username) != null)
                {
                    // Error id 400 - Bad Request, deoarece logica initiala trateaza username-ul duplicat ca request invalid.
                    return BadRequestError("Username already exists.");
                }

                if (string.IsNullOrEmpty(registerDto.Password) ||
                    !Regex.IsMatch(registerDto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))
                {
                    // Error id 400 - Bad Request, deoarece parola nu respecta regula de validare.
                    return BadRequestError("Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number.");
                }

                var validRoles = new List<string> { "Owner", "Administrator", "Participant" };

                if (!validRoles.Contains(registerDto.Persoana.Rol))
                {
                    // Error id 400 - Bad Request, deoarece rolul trimis nu este permis.
                    return BadRequestError("Invalid role. Allowed values: Owner, Administrator, Participant.");
                }

                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.Password);

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var persoana = new Persoana
                    {
                        Nume = registerDto.Persoana.Nume,
                        Prenume = registerDto.Persoana.Prenume,
                        Email = registerDto.Persoana.Email,
                        tipPersoana = registerDto.Persoana.TipPersoana,
                        Telefon = registerDto.Persoana.Telefon,
                        Rol = registerDto.Persoana.Rol
                    };

                    await _context.Persoane.AddAsync(persoana);
                    await _context.SaveChangesAsync();

                    var user = new User
                    {
                        Username = registerDto.Username,
                        Password = hashedPassword,
                        IdPersoana = persoana.IdPersoana
                    };

                    await _context.Users.AddAsync(user);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation($"User {registerDto.Username} registered successfully.");

                    return new OkObjectResult(new
                    {
                        message = "Account created successfully.",
                        user = new
                        {
                            user.IdUser,
                            user.Username,
                            persoana
                        }
                    });
                }
                catch (Exception txEx)
                {
                    await transaction.RollbackAsync();

                    _logger.LogError(txEx, "Transaction failed during user registration.");

                    // Error id 500 - Internal Server Error, deoarece tranzactia pentru creare user a esuat.
                    return InternalServerError("An error occurred while creating the user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the registration request.");

                // Error id 500 - Internal Server Error, deoarece inregistrarea a esuat neasteptat.
                return InternalServerError("An internal server error occurred.");
            }
        }

        public async Task<IActionResult> LoginResponse(LoginDto credentials)
        {
            try
            {
                var user = await GetByUsername(credentials.Username);

                if (user == null || !BCrypt.Net.BCrypt.Verify(credentials.Password, user.Password))
                {
                    // Error id 401 - Unauthorized, deoarece datele de autentificare sunt incorecte.
                    return UnauthorizedError("Invalid credentials");
                }

                var token = GenerateJwtToken(user);

                var response = new
                {
                    token,
                    user = new
                    {
                        user.Username,
                        Persoana = user.Persoana != null ? new
                        {
                            user.Persoana.Nume,
                            user.Persoana.Prenume,
                            user.Persoana.Rol
                        } : null
                    }
                };

                return new OkObjectResult(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the login request.");

                // Error id 500 - Internal Server Error, deoarece autentificarea a esuat neasteptat.
                return InternalServerError("An error occurred while processing your request.");
            }
        }

        public async Task<IActionResult> UpdateUserResponse(int id, UpdateUserDto updateDto)
        {
            if (updateDto == null || updateDto.Persoana == null)
            {
                // Error id 400 - Bad Request, deoarece lipsesc datele de user sau datele personale.
                return BadRequestError("Invalid request. Missing user or personal data.");
            }

            try
            {
                var user = await GetById(id);

                if (user == null)
                {
                    // Error id 404 - Not Found, deoarece userul care trebuie actualizat nu exista.
                    return NotFoundError("User not found.");
                }

                // Validate email format
                if (string.IsNullOrEmpty(updateDto.Persoana.Email) ||
                    !Regex.IsMatch(updateDto.Persoana.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    // Error id 400 - Bad Request, deoarece email-ul are format invalid.
                    return BadRequestError("Invalid email format.");
                }

                // Ensure email and username are not duplicates
                var existingEmailUser = await GetByEmail(updateDto.Persoana.Email);

                if (existingEmailUser != null && existingEmailUser.IdUser != id)
                {
                    // Error id 400 - Bad Request, deoarece logica initiala trateaza email-ul duplicat ca request invalid.
                    return BadRequestError("Email already exists.");
                }

                var existingUsernameUser = await GetByUsername(updateDto.Username);

                if (existingUsernameUser != null && existingUsernameUser.IdUser != id)
                {
                    // Error id 400 - Bad Request, deoarece logica initiala trateaza username-ul duplicat ca request invalid.
                    return BadRequestError("Username already exists.");
                }

                // Validate role
                var validRoles = new List<string> { "Owner", "Administrator", "Participant" };

                if (!validRoles.Contains(updateDto.Persoana.Rol))
                {
                    // Error id 400 - Bad Request, deoarece rolul trimis nu este permis.
                    return BadRequestError("Invalid role. Allowed values: Owner, Administrator, Participant.");
                }

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // Step 1: Update Persoana
                    var persoana = await _context.Persoane.FindAsync(user.IdPersoana);

                    if (persoana == null)
                    {
                        // Error id 404 - Not Found, deoarece datele personale asociate userului nu exista.
                        return NotFoundError("Associated personal data not found.");
                    }

                    persoana.Nume = updateDto.Persoana.Nume;
                    persoana.Prenume = updateDto.Persoana.Prenume;
                    persoana.Email = updateDto.Persoana.Email;
                    persoana.tipPersoana = updateDto.Persoana.TipPersoana;
                    persoana.Telefon = updateDto.Persoana.Telefon;
                    persoana.Rol = updateDto.Persoana.Rol;

                    _context.Persoane.Update(persoana);
                    await _context.SaveChangesAsync();

                    // Step 2: Update `User`
                    user.Username = updateDto.Username;

                    if (!string.IsNullOrEmpty(updateDto.Password))
                    {
                        user.Password = BCrypt.Net.BCrypt.HashPassword(updateDto.Password);
                    }

                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    _logger.LogInformation($"User {id} updated successfully.");

                    // Return updated user data (without password)
                    return new OkObjectResult(new
                    {
                        message = "User updated successfully.",
                        user = new
                        {
                            user.IdUser,
                            user.Username,
                            persoana
                        }
                    });
                }
                catch (Exception txEx)
                {
                    await transaction.RollbackAsync();

                    _logger.LogError(txEx, "Transaction failed during user update.");

                    // Error id 500 - Internal Server Error, deoarece tranzactia pentru update user a esuat.
                    return InternalServerError("An error occurred while updating the user.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the update request.");

                // Error id 500 - Internal Server Error, deoarece actualizarea userului a esuat neasteptat.
                return InternalServerError("An internal server error occurred.");
            }
        }

        public async Task<IActionResult> DeleteUserResponse(int id)
        {
            try
            {
                var result = await DeleteUser(id);

                if (!result)
                {
                    // Error id 404 - Not Found, deoarece userul nu exista sau este deja sters.
                    return NotFoundError("User not found or already deleted");
                }

                _logger.LogInformation($"User {id} deleted successfully.");

                return new OkObjectResult(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the delete request.");

                // Error id 500 - Internal Server Error, deoarece stergerea userului a esuat neasteptat.
                return InternalServerError("An error occurred while processing your request.");
            }
        }

        public async Task<IActionResult> UpdatePasswordResponse(int id, string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                // Error id 400 - Bad Request, deoarece parola lipseste sau are mai putin de 6 caractere.
                return BadRequestError("Password must be at least 6 characters long.");
            }

            try
            {
                var user = await GetById(id);

                if (user == null)
                {
                    // Error id 404 - Not Found, deoarece userul pentru care se schimba parola nu exista.
                    return NotFoundError("User not found.");
                }

                // Hash the new password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.Password = hashedPassword;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password updated successfully for user ID {id}");

                return new OkObjectResult(new { message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating password for user ID {id}");

                // Error id 500 - Internal Server Error, deoarece actualizarea parolei a esuat neasteptat.
                return InternalServerError("An error occurred while updating the password.");
            }
        }

        public async Task<IActionResult> ChangePasswordWithOldResponse(int id, AccountController.ChangePasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            {
                // Error id 400 - Bad Request, deoarece parola veche sau parola noua lipseste.
                return BadRequestError("Old and new passwords are required.");
            }

            if (dto.NewPassword.Length < 6)
            {
                // Error id 400 - Bad Request, deoarece parola noua are mai putin de 6 caractere.
                return BadRequestError("New password must be at least 6 characters long.");
            }

            try
            {
                var user = await GetById(id);

                if (user == null)
                {
                    // Error id 404 - Not Found, deoarece userul pentru care se schimba parola nu exista.
                    return NotFoundError("User not found.");
                }

                // Verificare parola veche
                if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
                {
                    // Error id 401 - Unauthorized, deoarece parola veche este incorecta.
                    return UnauthorizedError("Old password is incorrect.");
                }

                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password changed successfully for user ID {id}");

                return new OkObjectResult(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user ID {id}");

                // Error id 500 - Internal Server Error, deoarece schimbarea parolei a esuat neasteptat.
                return InternalServerError("An error occurred while changing the password.");
            }
        }

        public async Task<IActionResult> UpdateDetaliiUserResponse(int idUser, AccountController.UpdateDetaliiUserDTO dto)
        {
            try
            {
                var user = await _context.Users
                    .Include(u => u.Persoana)
                    .FirstOrDefaultAsync(u => u.IdUser == idUser);

                if (user == null)
                {
                    // Error id 404 - Not Found, deoarece utilizatorul nu exista.
                    return NotFoundError("Utilizatorul nu exista.");
                }

                // Update User (doar campurile nenule)
                if (!string.IsNullOrWhiteSpace(dto.Username))
                    user.Username = dto.Username;

                if (!string.IsNullOrWhiteSpace(dto.Password))
                    user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

                // Update Persoana (doar campurile nenule, fara TipPersoana si Rol!)
                if (user.Persoana != null)
                {
                    if (!string.IsNullOrWhiteSpace(dto.Nume))
                        user.Persoana.Nume = dto.Nume;

                    if (!string.IsNullOrWhiteSpace(dto.Prenume))
                        user.Persoana.Prenume = dto.Prenume;

                    if (!string.IsNullOrWhiteSpace(dto.Email))
                        user.Persoana.Email = dto.Email;

                    if (!string.IsNullOrWhiteSpace(dto.Telefon))
                        user.Persoana.Telefon = dto.Telefon;
                }

                await _context.SaveChangesAsync();

                return new OkObjectResult(new { message = "Detaliile au fost actualizate cu succes!" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating user details for user ID {idUser}");

                // Error id 500 - Internal Server Error, deoarece actualizarea detaliilor a esuat neasteptat.
                return InternalServerError("An error occurred while updating the user details.");
            }
        }

        public async Task<IActionResult> UpdateRolResponse(int id, string newRol)
        {
            try
            {
                // Cauta userul dupa Id
                var user = await _context.Users
                    .Include(u => u.Persoana)
                    .FirstOrDefaultAsync(u => u.IdUser == id);

                if (user == null || user.Persoana == null)
                {
                    // Error id 404 - Not Found, deoarece utilizatorul sau persoana asociata nu exista.
                    return NotFoundError("Utilizatorul nu a fost gasit!");
                }

                // Valideaza string-ul daca vrei (doar anumite roluri permise)
                var roluriPermise = new[] { "Owner", "Administrator", "Participant" };

                if (!roluriPermise.Contains(newRol))
                {
                    // Error id 400 - Bad Request, deoarece rolul trimis nu este permis.
                    return BadRequestError("Rol invalid!");
                }

                // Actualizeaza rolul in Persoana
                user.Persoana.Rol = newRol;
                await _context.SaveChangesAsync();

                return new OkObjectResult(new { message = $"Rolul a fost actualizat la: {newRol}" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating role for user ID {id}");

                // Error id 500 - Internal Server Error, deoarece actualizarea rolului a esuat neasteptat.
                return InternalServerError("An error occurred while updating the role.");
            }
        }

        private string GenerateJwtToken(User user)
        {
            try
            {
                var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
                var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

                var claims = new List<Claim>
                {
                    new Claim("IdUser", user.IdUser.ToString()),
                    new Claim("Username", user.Username ?? string.Empty),
                    new Claim("IdPersoana", user.IdPersoana.ToString())
                };

                if (user.Persoana != null)
                {
                    claims.Add(new Claim("Persoana.IdPersoana", user.Persoana.IdPersoana.ToString()));
                    claims.Add(new Claim("Persoana.Nume", user.Persoana.Nume ?? string.Empty));
                    claims.Add(new Claim("Persoana.Prenume", user.Persoana.Prenume ?? string.Empty));
                    claims.Add(new Claim("Persoana.Email", user.Persoana.Email ?? string.Empty));
                    claims.Add(new Claim("Persoana.tipPersoana", user.Persoana.tipPersoana ?? string.Empty));
                    claims.Add(new Claim("Persoana.Telefon", user.Persoana.Telefon ?? string.Empty));
                }

                var token = new JwtSecurityToken(
                    issuer: _configuration["Jwt:Issuer"],
                    audience: _configuration["Jwt:Audience"],
                    claims: claims,
                    expires: DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpirationInMinutes"])),
                    signingCredentials: creds);

                return new JwtSecurityTokenHandler().WriteToken(token);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while generating JWT token.");
                throw;
            }
        }

        private IActionResult BadRequestError(string message)
        {
            return new BadRequestObjectResult(new
            {
                errorId = 400,
                error = "Bad Request",
                message
            });
        }

        private IActionResult UnauthorizedError(string message)
        {
            return new UnauthorizedObjectResult(new
            {
                errorId = 401,
                error = "Unauthorized",
                message
            });
        }

        private IActionResult NotFoundError(string message)
        {
            return new NotFoundObjectResult(new
            {
                errorId = 404,
                error = "Not Found",
                message
            });
        }

        private IActionResult InternalServerError(string message)
        {
            return new ObjectResult(new
            {
                errorId = 500,
                error = "Internal Server Error",
                message
            })
            {
                StatusCode = 500
            };
        }
    }
}