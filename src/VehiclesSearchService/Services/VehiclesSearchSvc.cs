using Newtonsoft.Json;
using System.Reflection;
using VehiclesSearchService.Models;
using VehiclesSearchService.ViewModels;

namespace VehiclesSearchService.Services
{
    public class VehiclesSearchSvc : IVehiclesSearchSvc
    {
        private bool FilterByMfrName(List<DetailedManufacturer> manufacturers, string requiredName)
        {
            var compOpt = StringComparison.InvariantCultureIgnoreCase;

            manufacturers.RemoveAll(mfr => 
                !mfr.MfrFullName.Contains(requiredName, compOpt) ||
                !mfr.MfrCommonName.Contains(requiredName, compOpt));

            return manufacturers.Count != 0 ? true : false;
        }

        private bool FilterByCountry(List<DetailedManufacturer> manufacturers, string country)
        {
            manufacturers.RemoveAll(mfr =>
                !mfr.Country.Contains(country, StringComparison.InvariantCultureIgnoreCase));

            return manufacturers.Count != 0 ? true : false;
        }

        private bool FilterByCity(List<DetailedManufacturer> manufacturers, string city)
        {
            manufacturers.RemoveAll(mfr =>
                !mfr.Country.Contains(city, StringComparison.InvariantCultureIgnoreCase));

            return manufacturers.Count != 0 ? true : false;
        }

        private bool FilterByState(List<DetailedManufacturer> manufacturers, string state)
        {
            manufacturers.RemoveAll(mfr =>
                !mfr.State.Contains(state, StringComparison.InvariantCultureIgnoreCase));

            return manufacturers.Count != 0 ? true : false;
        }

        public List<DetailedManufacturer> GetFilteredMfrs(string response, MfrSearchViewModel searchCriteria)
        {
            MfrDetailsResponse<DetailedManufacturer, DetailedVehicleType>? parsedResponse;
            if (!MfrDetailsResponse<DetailedManufacturer, DetailedVehicleType>.TryParseResponse(response, out parsedResponse))
                throw new Exception();

            var manufacturers = parsedResponse!.Results;

            if (!FilterByMfrName(manufacturers, searchCriteria.MfrName))
                return manufacturers;

            if (!FilterByCountry(manufacturers, searchCriteria.Country))
                return manufacturers;

            if (!FilterByCity(manufacturers, searchCriteria.City))
                return manufacturers;

            if (!FilterByState(manufacturers, searchCriteria.State))
                return manufacturers;

            return manufacturers;
        }
    }
}
