using System.Threading.Tasks;
using LicentaInAngular.Server.Interfaces;

namespace LicentaInAngular.Server.Tests.IntegrationTests
{
    public class FakeGPTService : IGPTService
    {
        public Task<string> GetChatResponse(string prompt)
        {
            return Task.FromResult("Raspuns fake pentru test.");
        }
    }
}
