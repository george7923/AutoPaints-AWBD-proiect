using LicentaInAngular.Server.Models;

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
    }
}
