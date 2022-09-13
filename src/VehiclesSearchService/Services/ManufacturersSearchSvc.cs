using Common.CustomExceptions;
using Common.Utilities;
using Common.ViewModels;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Reflection;
using VehiclesSearchService.Controllers;
using VehiclesSearchService.Models;
using VehiclesSearchService.Utilities;
using VehiclesSearchService.ViewModels;

namespace VehiclesSearchService.Services
{
    public class ManufacturersSearchSvc : IManufacturersSearchSvc
    {
        private readonly HttpClient _httpClient;
        private IOptions<NhtsaApiConfig> _nhtsaApiConfig;
        private readonly ILogger<ManufacturersSearchSvc> _logger;

        public ManufacturersSearchSvc(IOptions<NhtsaApiConfig> nhtsaApiConfig, ILogger<ManufacturersSearchSvc> logger)
        {
            _nhtsaApiConfig = nhtsaApiConfig;

            _httpClient = new();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_nhtsaApiConfig.Value.RequestMediaType));
            _httpClient.BaseAddress = new Uri($@"{_nhtsaApiConfig.Value.ApiUrl}/");

            _logger = logger;
        }

        public async Task<List<DetailedManufacturer>> GetDetailedMfrsAsync(MfrSearchViewModel searchCriteria)
        {
            var appropriateProp = GetFirstAppropriateProperty(searchCriteria);
            List<DetailedManufacturer>? mfrs;

            switch (appropriateProp)
            {
                case nameof(MfrSearchViewModel.MfrName):
                    mfrs = await GetByIdentifiersAsync(searchCriteria.MfrName, searchCriteria); break;

                case nameof(MfrSearchViewModel.MfrId):
                    mfrs = await GetByIdentifiersAsync(searchCriteria!.MfrId!.ToString()!, searchCriteria); break;

                case nameof(MfrSearchViewModel.Country):
                case nameof(MfrSearchViewModel.City):
                case nameof(MfrSearchViewModel.State):
                    mfrs = await GetBySecondaryCreteriasAsync(searchCriteria); break;

                default:
                    throw new Exception(LogEventType.MfrsInfoRequestFailed.Replace("params", searchCriteria.AllPropertiesValues()));
            }

            return mfrs;
        }

        /// <summary>
        /// Searchs for the first non-empty/non-null property
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns>First filled property of provided model</returns>
        private static string GetFirstAppropriateProperty(MfrSearchViewModel searchCriteria)
        {
            var filledProperties = PropertyLookup.GetFilledProperties(searchCriteria);

            if (!filledProperties.Any())
                throw new InvalidParamsException(LogEventType.ParameterMissed.Replace("argument", searchCriteria.AllPropertiesValues()));

            var containsNameOrId = filledProperties.Where(p =>
                (p.Name == nameof(MfrSearchViewModel.MfrName) ||
                p.Name == nameof(MfrSearchViewModel.MfrId)));

            // At first we're looking for MfrId or MfrName, because searching using these params are more efficient
            if (containsNameOrId.Any())
                return containsNameOrId.First().Name;

            foreach (var prop in filledProperties)
            {
                switch (prop.Name)
                {
                    case nameof(MfrSearchViewModel.Country):
                        return nameof(MfrSearchViewModel.Country);

                    case nameof(MfrSearchViewModel.City):
                        return nameof(MfrSearchViewModel.City);

                    default:
                        break;
                }
            }

            return string.Empty;
        }

        // Filtration methods section below
        private static bool FilterByMfrName(List<DetailedManufacturer> manufacturers, string requiredName)
        {
            var compOpt = StringComparison.InvariantCultureIgnoreCase;

            manufacturers.RemoveAll(mfr => 
                !mfr.MfrFullName.Contains(requiredName, compOpt) ||
                !mfr.MfrCommonName.Contains(requiredName, compOpt));

            return manufacturers.Count != 0 ? true : false;
        }

        private static bool FilterByCountry(List<DetailedManufacturer> manufacturers, string country)
        {
            manufacturers.RemoveAll(mfr =>
                !mfr.Country.Contains(country, StringComparison.InvariantCultureIgnoreCase));

            return manufacturers.Count != 0 ? true : false;
        }

        private static bool FilterByCity(List<DetailedManufacturer> manufacturers, string city)
        {
            manufacturers.RemoveAll(mfr =>
                !mfr.Country.Contains(city, StringComparison.InvariantCultureIgnoreCase));

            return manufacturers.Count != 0 ? true : false;
        }

        private static bool FilterByState(List<DetailedManufacturer> manufacturers, string state)
        {
            manufacturers.RemoveAll(mfr =>
                !mfr.State.Contains(state, StringComparison.InvariantCultureIgnoreCase));

            return manufacturers.Count != 0 ? true : false;
        }

        private static List<DetailedManufacturer> GetFilteredMfrs(string response, MfrSearchViewModel searchCriteria)
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
        // End of filtration methods section
        
        private async IAsyncEnumerable<List<BaseManufacturer>> QueueAllMfrsAsync(string uri)
        {
            int counter = 1;
            bool isLastEmpty = false;
            do
            {
                string response = await GetResponseAsync($"{uri}&page={counter++}");

                var newMfrs = MfrDetailsResponse<BaseManufacturer, BaseVehicleType>.TryParseResponse(response)?.Results;

                if (newMfrs is null || newMfrs.Count == 0)
                    isLastEmpty = true;
                else
                    yield return newMfrs;

            } while (!isLastEmpty);
        }

        // Request methods section below
        private async Task<string> GetResponseAsync(string path)
        {
            var response = await _httpClient.GetAsync(path);

            return await response.Content.ReadAsStringAsync();
        }

        private async Task<List<DetailedManufacturer>> GetByIdentifiersAsync(string identifier, MfrSearchViewModel searchCreteria)
        {
            string uri = $@"GetManufacturerDetails/{identifier}{_nhtsaApiConfig.Value.RequestPathFormating}";
            var response = await GetResponseAsync(uri);

            var filteredResult = GetFilteredMfrs(response, searchCreteria);

            return filteredResult;
        }

        private async Task<List<DetailedManufacturer>> GetBySecondaryCreteriasAsync(MfrSearchViewModel searchCreteria)
        {
            string uri = $@"GetAllManufacturers{_nhtsaApiConfig.Value.RequestPathFormating}";

            List<DetailedManufacturer> filteredManufacturers = new();

            await foreach (var mfrs in QueueAllMfrsAsync(uri))
            foreach (var mfr in mfrs)
            {
                var newFilteredMfrs = await GetByIdentifiersAsync(mfr.MfrId.ToString(), searchCreteria);
                if (newFilteredMfrs.Any())
                    filteredManufacturers.AddRange(newFilteredMfrs);
            }

            return filteredManufacturers;
        }
        // End of request methods section 
    }
}
