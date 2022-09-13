using VehiclesSearchService.Models;
using VehiclesSearchService.ViewModels;

namespace VehiclesSearchService.Services
{
    public interface IManufacturersSearchSvc
    {
        public Task<List<DetailedManufacturer>> GetDetailedMfrsAsync(MfrSearchViewModel searchCriteria);
    }
}
