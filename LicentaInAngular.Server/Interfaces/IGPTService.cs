using System.Threading.Tasks;

namespace LicentaInAngular.Server.Interfaces
{
    public interface IGPTService
    {
        Task<string> GetChatResponse(string prompt);
    }
}
