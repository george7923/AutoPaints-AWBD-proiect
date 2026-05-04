using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using LicentaInAngular.Server.Models;
using LicentaInAngular.Server.Repositories;
using LicentaInAngular.Server.Data;
using BCrypt.Net;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.IdentityModel.Tokens.Jwt;
using LicentaInAngular.Server.DataLayer.DTO;
using Microsoft.EntityFrameworkCore;

namespace LicentaInAngular.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IPersoanaRepository _persoanaRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController> _logger;
        private readonly ApplicationDbContext _context;

        public AccountController(
            IUserRepository userRepository,
            IPersoanaRepository persoanaRepository,
            IConfiguration configuration,
            ILogger<AccountController> logger,
            ApplicationDbContext context)
        {
            _userRepository = userRepository;
            _persoanaRepository = persoanaRepository;
            _configuration = configuration;
            _logger = logger;
            _context = context;
        }
        [HttpGet("get-id/{username}")]
        public async Task<IActionResult> GetUserIdByUsername(string username)
        {
            var userId = await _userRepository.GetUserIdByUsername(username);
            if (userId == null)
            {
                return NotFound(new { error = "User not found" });
            }
            return Ok(new { IdUser = userId });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            // Fără repository, direct pe ApplicationDbContext
            var users = await _context.Users
                .Select(u => new { IdUser = u.IdUser, Username = u.Username })
                .ToListAsync();

            return Ok(users);
        }


        [HttpGet("User/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userRepository.GetByUsername(username);
                if (user == null)
                    return NotFound(new { error = "User not found" });

                return Ok(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the user details.");
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }
        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            if (registerDto == null || registerDto.Persoana == null)
                return BadRequest(new { error = "Invalid request. Missing user or personal data." });

            try
            {
                if (string.IsNullOrEmpty(registerDto.Persoana.Email) ||
                    !Regex.IsMatch(registerDto.Persoana.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return BadRequest(new { error = "Invalid email format." });


                if (await _userRepository.GetByEmail(registerDto.Persoana.Email) != null)
                    return BadRequest(new { error = "Email already exists." });

                if (await _userRepository.GetByUsername(registerDto.Username) != null)
                    return BadRequest(new { error = "Username already exists." });


                if (string.IsNullOrEmpty(registerDto.Password) || !Regex.IsMatch(registerDto.Password, @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$"))

                    return BadRequest(new { error = "Password must be at least 8 characters long and contain at least one uppercase letter, one lowercase letter, and one number." });


                var validRoles = new List<string> { "Owner", "Administrator", "Participant" };
                if (!validRoles.Contains(registerDto.Persoana.Rol))
                    return BadRequest(new { error = "Invalid role. Allowed values: Owner, Administrator, Participant." });
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
                    return Ok(new
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
                    return StatusCode(500, new { error = "An error occurred while creating the user." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the registration request.");
                return StatusCode(500, new { error = "An internal server error occurred." });
            }
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto credentials)
        {
            try
            {
                var user = await _userRepository.GetByUsername(credentials.Username);
                if (user == null || !BCrypt.Net.BCrypt.Verify(credentials.Password, user.Password))
                    return Unauthorized(new { error = "Invalid credentials" });

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

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the login request.");
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }


        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
        {
            if (updateDto == null || updateDto.Persoana == null)
                return BadRequest(new { error = "Invalid request. Missing user or personal data." });

            try
            {
                var user = await _userRepository.GetById(id);
                if (user == null)
                    return NotFound(new { error = "User not found." });

                // Validate email format
                if (string.IsNullOrEmpty(updateDto.Persoana.Email) ||
                    !Regex.IsMatch(updateDto.Persoana.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    return BadRequest(new { error = "Invalid email format." });

                // Ensure email and username are not duplicates
                var existingEmailUser = await _userRepository.GetByEmail(updateDto.Persoana.Email);
                if (existingEmailUser != null && existingEmailUser.IdUser != id)
                    return BadRequest(new { error = "Email already exists." });

                var existingUsernameUser = await _userRepository.GetByUsername(updateDto.Username);
                if (existingUsernameUser != null && existingUsernameUser.IdUser != id)
                    return BadRequest(new { error = "Username already exists." });

                // Validate role
                var validRoles = new List<string> { "Owner", "Administrator", "Participant" };
                if (!validRoles.Contains(updateDto.Persoana.Rol))
                    return BadRequest(new { error = "Invalid role. Allowed values: Owner, Administrator, Participant." });

                using var transaction = await _context.Database.BeginTransactionAsync();
                try
                {
                    // Step 1: Update Persoana
                    var persoana = await _context.Persoane.FindAsync(user.IdPersoana);
                    if (persoana == null)
                        return NotFound(new { error = "Associated personal data not found." });

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
                    return Ok(new
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
                    return StatusCode(500, new { error = "An error occurred while updating the user." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the update request.");
                return StatusCode(500, new { error = "An internal server error occurred." });
            }
        }


        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userRepository.DeleteUser(id);
                if (!result)
                    return NotFound(new { error = "User not found or already deleted" });

                _logger.LogInformation($"User {id} deleted successfully.");
                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing the delete request.");
                return StatusCode(500, new { error = "An error occurred while processing your request." });
            }
        }
        [HttpPut("UpdatePassword/{id}")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] string newPassword)
        {
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
                return BadRequest(new { error = "Password must be at least 6 characters long." });

            try
            {
                var user = await _userRepository.GetById(id);
                if (user == null)
                    return NotFound(new { error = "User not found." });

                // Hash the new password
                string hashedPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);
                user.Password = hashedPassword;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password updated successfully for user ID {id}");
                return Ok(new { message = "Password updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating password for user ID {id}");
                return StatusCode(500, new { error = "An error occurred while updating the password." });
            }
        }
        [HttpPut("ChangePasswordWithOld/{id}")]
        public async Task<IActionResult> ChangePasswordWithOld(int id, [FromBody] ChangePasswordDto dto)
        {
            if (dto == null || string.IsNullOrWhiteSpace(dto.OldPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return BadRequest(new { error = "Old and new passwords are required." });

            if (dto.NewPassword.Length < 6)
                return BadRequest(new { error = "New password must be at least 6 characters long." });

            try
            {
                var user = await _userRepository.GetById(id);
                if (user == null)
                    return NotFound(new { error = "User not found." });

                // Verificare parolă veche
                if (!BCrypt.Net.BCrypt.Verify(dto.OldPassword, user.Password))
                    return Unauthorized(new { error = "Old password is incorrect." });

                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                _logger.LogInformation($"Password changed successfully for user ID {id}");
                return Ok(new { message = "Password changed successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error changing password for user ID {id}");
                return StatusCode(500, new { error = "An error occurred while changing the password." });
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
        [HttpPut("update-detalii/{idUser}")]
        public async Task<IActionResult> UpdateDetaliiUser(int idUser, [FromBody] UpdateDetaliiUserDTO dto)
        {
            var user = await _context.Users
                .Include(u => u.Persoana)
                .FirstOrDefaultAsync(u => u.IdUser == idUser);
            if (user == null)
                return NotFound("Utilizatorul nu există.");

            // Update User (doar campurile nenule)
            if (!string.IsNullOrWhiteSpace(dto.Username))
                user.Username = dto.Username;

            if (!string.IsNullOrWhiteSpace(dto.Password))
                user.Password = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            // Update Persoana (doar campurile nenule, fără TipPersoana și Rol!)
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
            return Ok(new { message = "Detaliile au fost actualizate cu succes!" });
        }
        [HttpPut("update-rol/{id}")]
        public async Task<IActionResult> UpdateRol(int id, [FromBody] string newRol)
        {
            // Caută userul după Id
            var user = await _context.Users
                .Include(u => u.Persoana)
                .FirstOrDefaultAsync(u => u.IdUser == id);

            if (user == null || user.Persoana == null)
                return NotFound(new { message = "Utilizatorul nu a fost găsit!" });

            // Validează string-ul daca vrei (doar anumite roluri permise)
            var roluriPermise = new[] { "Owner", "Administrator", "Participant" };
            if (!roluriPermise.Contains(newRol))
                return BadRequest(new { message = "Rol invalid!" });

            // Actualizează rolul în Persoana
            user.Persoana.Rol = newRol;
            await _context.SaveChangesAsync();

            return Ok(new { message = $"Rolul a fost actualizat la: {newRol}" });
        }



        //                        new Claim("Password", user.Password ?? string.Empty),
        public class ChangePasswordDto
        {
            public string OldPassword { get; set; }
            public string NewPassword { get; set; }
        }
        public class UpdateDetaliiUserDTO
        {
            public int? IdUser { get; set; }
            public string? Username { get; set; }
            public string? Password { get; set; }
            public int? IdPersoana { get; set; }
            public string? Nume { get; set; }
            public string? Prenume { get; set; }
            public string? Email { get; set; }
            public string? Telefon { get; set; }
        }


    }
}

