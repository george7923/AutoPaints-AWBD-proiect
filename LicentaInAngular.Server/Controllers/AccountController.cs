using Microsoft.AspNetCore.Mvc;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Repositories;

namespace LicentaInAngular.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly UserService _userService;

        public AccountController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet("get-id/{username}")]
        public async Task<IActionResult> GetUserIdByUsername(string username)
        {
            return await _userService.GetUserIdByUsernameResponse(username);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            return await _userService.GetAllUsersResponse();
        }

        [HttpGet("User/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            return await _userService.GetUserByUsernameResponse(username);
        }

        [HttpPost("Register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto registerDto)
        {
            return await _userService.RegisterResponse(registerDto);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login([FromBody] LoginDto credentials)
        {
            return await _userService.LoginResponse(credentials);
        }

        [HttpPut("Update/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateDto)
        {
            return await _userService.UpdateUserResponse(id, updateDto);
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            return await _userService.DeleteUserResponse(id);
        }

        [HttpPut("UpdatePassword/{id}")]
        public async Task<IActionResult> UpdatePassword(int id, [FromBody] string newPassword)
        {
            return await _userService.UpdatePasswordResponse(id, newPassword);
        }

        [HttpPut("ChangePasswordWithOld/{id}")]
        public async Task<IActionResult> ChangePasswordWithOld(int id, [FromBody] ChangePasswordDto dto)
        {
            return await _userService.ChangePasswordWithOldResponse(id, dto);
        }

        [HttpPut("update-detalii/{idUser}")]
        public async Task<IActionResult> UpdateDetaliiUser(int idUser, [FromBody] UpdateDetaliiUserDTO dto)
        {
            return await _userService.UpdateDetaliiUserResponse(idUser, dto);
        }

        [HttpPut("update-rol/{id}")]
        public async Task<IActionResult> UpdateRol(int id, [FromBody] string newRol)
        {
            return await _userService.UpdateRolResponse(id, newRol);
        }

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