using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using hoistmt.Models.httpModels;

namespace hoistmt.HttpClients
{
    public class RegoSearch
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<RegoSearch> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        public RegoSearch(HttpClient httpClient, ILogger<RegoSearch> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
        }

        public async Task<RegoData> GetDataAsync(string rego)
        {
            string url = $"https://test.carjam.co.nz/a/vehicle:abcd?key=BE3F690788AA069ED2357D3D49E4F75443917F2C&plate={rego}";

            try
            {
                _logger.LogInformation("Sending request to {Url}", url);

                // Make the HTTP request
                HttpResponseMessage response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode(); // Throw if not a success code

                // Read the response content as a string
                string jsonResponse = await response.Content.ReadAsStringAsync();

                // Deserialize the JSON response into a C# object
                var data = JsonSerializer.Deserialize<RegoData>(jsonResponse, _jsonOptions);

                return data;
            }
            catch (HttpRequestException e)
            {
                _logger.LogError("Request error: {Message}", e.Message);
            }
            catch (JsonException e)
            {
                _logger.LogError("JSON error: {Message}", e.Message);
            }
            catch (Exception e)
            {
                _logger.LogError("Unexpected error: {Message}", e.Message);
            }

            return null;
        }
    }
}
