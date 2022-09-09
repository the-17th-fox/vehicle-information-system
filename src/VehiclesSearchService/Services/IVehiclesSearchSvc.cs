using VehiclesSearchService.Models;
using VehiclesSearchService.ViewModels;

namespace VehiclesSearchService.Services
{
    public interface IVehiclesSearchSvc
    {
        public List<DetailedManufacturer> GetFilteredMfrs(string response, MfrSearchViewModel searchCriteria);
    }
}
