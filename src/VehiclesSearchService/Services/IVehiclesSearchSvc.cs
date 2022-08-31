using VehiclesSearchService.Models;
using VehiclesSearchService.ViewModels;

namespace VehiclesSearchService.Services
{
    public interface IVehiclesSearchSvc
    {
        public List<Manufacturer> ProvideManufacturersCheckout(string response, ManufacturerSearchViewModel searchCriteria);
    }
}
