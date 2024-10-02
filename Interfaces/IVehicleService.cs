using hoistmt.Models;

namespace hoistmt.Interfaces;

public interface IVehicleService
{
    Task<IEnumerable<Vehicle>> GetVehiclesAsync();
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
    Task<Vehicle> GetVehicleDetails(int id);
    Task<Vehicle> DeleteVehicle(int vehicleId);
    Task<Vehicle> UpdateVehicle(Vehicle vehicle);
}