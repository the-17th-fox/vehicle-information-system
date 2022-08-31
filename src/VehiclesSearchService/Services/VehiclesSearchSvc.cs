using Newtonsoft.Json;
using VehiclesSearchService.Models;
using VehiclesSearchService.ViewModels;

namespace VehiclesSearchService.Services
{
    public class VehiclesSearchSvc : IVehiclesSearchSvc
    {
        private bool TryParseResponse(string response, out ManufacturerDetailsResponse? result)
        {
            var options = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            result = JsonConvert.DeserializeObject<ManufacturerDetailsResponse>(response, options);

            return result is not null ? true : false;
        }

        public List<Manufacturer> ProvideManufacturersCheckout(string response, ManufacturerSearchViewModel searchCriteria)
        {
            ManufacturerDetailsResponse? parsedResponse;
            if (!TryParseResponse(response, out parsedResponse))
                throw new Exception();

            var manufacturers = parsedResponse!.Results;

            if (!FilterByMfrName(manufacturers, searchCriteria.MfrName))
                return manufacturers;

            if (!FilterByCountry(manufacturers, searchCriteria.Country))
                return manufacturers;

            if(!FilterByCity(manufacturers, searchCriteria.City))
                return manufacturers;

            if (!FilterByState(manufacturers, searchCriteria.State))
                return manufacturers;

            return manufacturers;
        }

        private bool FilterByMfrName(List<Manufacturer> manufacturers, string requiredName)
        {
            var compOpt = StringComparison.InvariantCultureIgnoreCase;

            manufacturers.RemoveAll(mfr => 
                !mfr.MfrFullName.Contains(requiredName, compOpt) ||
                !mfr.MfrCommonName.Contains(requiredName, compOpt));

            return manufacturers.Count != 0 ? true : false;
        }

        private bool FilterByCountry(List<Manufacturer> manufacturers, string country)
        {
            manufacturers.RemoveAll(mfr =>
                !mfr.Country.Contains(country, StringComparison.InvariantCultureIgnoreCase));

            return manufacturers.Count != 0 ? true : false;
        }

        private bool FilterByCity(List<Manufacturer> manufacturers, string city)
        {
            manufacturers.RemoveAll(mfr =>
                !mfr.Country.Contains(city, StringComparison.InvariantCultureIgnoreCase));

            return manufacturers.Count != 0 ? true : false;
        }

        private bool FilterByState(List<Manufacturer> manufacturers, string state)
        {
            manufacturers.RemoveAll(mfr =>
                !mfr.State.Contains(state, StringComparison.InvariantCultureIgnoreCase));

            return manufacturers.Count != 0 ? true : false;
        }
    }
}
