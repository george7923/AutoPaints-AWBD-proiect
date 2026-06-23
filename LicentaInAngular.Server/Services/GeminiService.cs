using LicentaInAngular.Server.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace LicentaInAngular.Server.Services
{
    public class GeminiService : IGPTService
    {
        private readonly string _apiKey;
        private readonly string _model;
        private readonly HttpClient _httpClient;

        public GeminiService(IConfiguration configuration)
        {
            _apiKey = configuration["Gemini:ApiKey"];
            _model = configuration["Gemini:Model"] ?? "gemini-2.0-flash";

            if (string.IsNullOrWhiteSpace(_apiKey))
            {
                throw new InvalidOperationException("Gemini API key lipseste din configurare.");
            }

            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
            };
        }

        public async Task<string> GetChatResponse(string prompt)
        {
            var requestBody = new
            {
                systemInstruction = new
                {
                    parts = new[]
                    {
                        new
                        {
                            text = "Esti un asistent virtual pentru aplicatia AutoPaints. Raspunde clar, util si in limba romana, fara diacritice."
                        }
                    }
                },
                contents = new[]
                {
                    new
                    {
                        role = "user",
                        parts = new[]
                        {
                            new { text = prompt }
                        }
                    }
                },
                generationConfig = new
                {
                    temperature = 0.7,
                    maxOutputTokens = 500
                }
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var endpoint = $"v1beta/models/{_model}:generateContent?key={_apiKey}";
            var response = await _httpClient.PostAsync(endpoint, content);

            var responseContent = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"Gemini request failed with status code {response.StatusCode}: {responseContent}");
            }

            using var document = JsonDocument.Parse(responseContent);
            var root = document.RootElement;

            if (!root.TryGetProperty("candidates", out var candidates) || candidates.GetArrayLength() == 0)
            {
                return "Nu am primit un raspuns valid de la Gemini.";
            }

            var firstCandidate = candidates[0];

            if (!firstCandidate.TryGetProperty("content", out var contentElement) ||
                !contentElement.TryGetProperty("parts", out var parts) ||
                parts.GetArrayLength() == 0)
            {
                return "Gemini nu a returnat continut text.";
            }

            if (parts[0].TryGetProperty("text", out var textElement))
            {
                return textElement.GetString() ?? "Raspuns gol primit de la Gemini.";
            }

            return "Gemini nu a returnat text in formatul asteptat.";
        }
    }
}