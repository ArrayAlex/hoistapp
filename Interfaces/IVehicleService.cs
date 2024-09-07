using hoistmt.Models;

namespace hoistmt.Interfaces;

public interface IVehicleService
{
    Task<IEnumerable<Vehicle>> GetVehiclesAsync();
}