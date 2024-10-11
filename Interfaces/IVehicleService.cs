using hoistmt.Models;

namespace hoistmt.Interfaces;

public interface IVehicleService
{
    Task<IEnumerable<Vehicle>> GetVehiclesAsync();
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
    Task<Vehicle> GetVehicleDetails(int id);
    Task<IEnumerable<Vehicle>> SearchVehicles(string searchTerm);  // Changed return type
    Task<Vehicle> DeleteVehicle(int vehicleId);
    Task<Vehicle> UpdateVehicle(Vehicle vehicle);
}