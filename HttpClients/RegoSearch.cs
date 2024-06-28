
using System.Text.Json;

using Microsoft.EntityFrameworkCore;
using hoistmt.Data;
using hoistmt.Functions;
using hoistmt.Models.httpModels;
using hoistmt.Models.MasterDbModels;

namespace hoistmt.HttpClients;

public class RegoSearch
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RegoSearch> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly MasterDbContext _context;
    private readonly Credits _credits;

    public RegoSearch(HttpClient httpClient, ILogger<RegoSearch> logger, MasterDbContext context, Credits credits)
    {
        _httpClient = httpClient;
        _context = context;
        _logger = logger;
        _credits = credits;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };
    }

    public async Task<(RegoData data, string error)> GetDataAsync(string rego, string companyId)
    {
        // Check if the company has enough credits
        if (!_credits.hasCredits(companyId))
        {
            _logger.LogError("Company does not have enough credits.");
            return (null, "Not enough credits.");
        }

        // Check if the vehicle is already in the database
        var existingVehicle = await _context.vehicledata.AsNoTracking()
            .FirstOrDefaultAsync(v => v.plate == rego);
        if (existingVehicle != null)
        {
            _logger.LogInformation("Vehicle found in the database.");
            if (await _credits.TryDeductCreditsAsync(companyId, 0.15)) // Deduct 15 cents
            {
                return (ConvertToRegoData(existingVehicle), null);
            }
            else
            {
                _logger.LogError("Not enough credits to deduct 15 cents.");
                return (null, "Not enough credits.");
            }
        }

        _logger.LogInformation("Vehicle not found in the database. Fetching from API...");

        // If not found, fetch from API
        string url = $"https://test.carjam.co.nz/a/vehicle:abcd?key=BE3F690788AA069ED2357D3D49E4F75443917F2C&plate={rego}";
        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throw if not a success code

            string jsonResponse = await response.Content.ReadAsStringAsync();
            _logger.LogInformation($"API response: {jsonResponse}");

            var data = JsonSerializer.Deserialize<RegoData>(jsonResponse, _jsonOptions);

            // Ensure deserialized object is not null
            if (data == null)
            {
                _logger.LogError("Deserialized vehicle data is null.");
                return (null, "Deserialized vehicle data is null.");
            }

            // Save the fetched data to the database
            var vehicle = ConvertToVehicle(data);
            _context.vehicledata.Add(vehicle);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Vehicle data fetched from API and saved to the database.");
            if (await _credits.TryDeductCreditsAsync(companyId, 0.21)) // Deduct 21 cents
            {
                return (data, null);
            }
            else
            {
                _logger.LogError("Not enough credits to deduct 21 cents.");
                return (null, "Not enough credits.");
            }
        }
        catch (HttpRequestException e)
        {
            _logger.LogError("Request error: {Message}", e.Message);
            return (null, "Request error.");
        }
        catch (JsonException e)
        {
            _logger.LogError("JSON error: {Message}", e.Message);
            return (null, "JSON error.");
        }
        catch (Exception e)
        {
            _logger.LogError("Unexpected error: {Message}", e.Message);
            return (null, "Unexpected error.");
        }
    }

    private RegoData ConvertToRegoData(Vehicle vehicle)
    {
        // Assuming the data is already in the correct format in Vehicle class
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(vehicle, options);
        return JsonSerializer.Deserialize<RegoData>(json, options);
    }

    private Vehicle ConvertToVehicle(RegoData data)
    {
        // Assuming the data is already in the correct format in RegoData class
        var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
        var json = JsonSerializer.Serialize(data, options);
        return JsonSerializer.Deserialize<Vehicle>(json, options);
    }
}