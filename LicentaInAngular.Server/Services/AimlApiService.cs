using LicentaInAngular.Server.Interfaces;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace LicentaInAngular.Server.Services
{
    public class AimlApiService : IGPTService
    {
        private readonly string _apiKey;
        private readonly HttpClient _httpClient;

        public AimlApiService(IConfiguration configuration)
        {

            _apiKey = configuration["AIMLAPI:ApiKey"] ?? "426fb7071357424ca14b518a2ee86af0";
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://api.aimlapi.com/v1/")
            };
            _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
        }

        public async Task<string> GetChatResponse(string prompt)
        {

            var requestBody = new
            {
                model = "gpt-4o",
                messages = new[]
                {
                    new { role = "system", content = "Ești un asistent virtual." },
                    new { role = "user", content = prompt }
                },
                max_tokens = 100,
                temperature = 0.7
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("chat/completions", content);
            response.EnsureSuccessStatusCode();
            var responseContent = await response.Content.ReadAsStringAsync();
            var responseData = JsonSerializer.Deserialize<JsonElement>(responseContent);
            return responseData.GetProperty("choices")[0]
                               .GetProperty("message")
                               .GetProperty("content")
                               .GetString();
        }
    }
}
