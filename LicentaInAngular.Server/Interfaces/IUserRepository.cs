using LicentaInAngular.Server.Controllers;
using LicentaInAngular.Server.DataLayer.DTO;
using LicentaInAngular.Server.Models;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Repositories
{
    public interface IUserRepository
    {
        Task<int?> GetUserIdByUsername(string username);

        Task<User?> GetByUsername(string username);

        Task<User?> GetByEmail(string email);

        Task CreateUser(User user);

        Task<User?> UpdateUser(User user);

        Task<bool> DeleteUser(int idUser);

        Task<User> GetById(int id);

        // Metode Response mutate din AccountController
        Task<IActionResult> GetUserIdByUsernameResponse(string username);

        Task<IActionResult> GetAllUsersResponse();

        Task<IActionResult> GetUserByUsernameResponse(string username);

        Task<IActionResult> RegisterResponse(RegisterUserDto registerDto);

        Task<IActionResult> LoginResponse(LoginDto credentials);

        Task<IActionResult> UpdateUserResponse(int id, UpdateUserDto updateDto);

        Task<IActionResult> DeleteUserResponse(int id);

        Task<IActionResult> UpdatePasswordResponse(int id, string newPassword);

        Task<IActionResult> ChangePasswordWithOldResponse(int id, AccountController.ChangePasswordDto dto);

        Task<IActionResult> UpdateDetaliiUserResponse(int idUser, AccountController.UpdateDetaliiUserDTO dto);

        Task<IActionResult> UpdateRolResponse(int id, string newRol);
    }
}
