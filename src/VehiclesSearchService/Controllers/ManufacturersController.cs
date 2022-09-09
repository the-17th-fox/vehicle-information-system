using Common.CustomExceptions;
using Common.Utilities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.Extensions.Options;
using System;
using System.Data.SqlTypes;
using System.IO.IsolatedStorage;
using System.Net;
using System.Net.Http.Headers;
using VehiclesSearchService.Models;
using VehiclesSearchService.Services;
using VehiclesSearchService.Utilities;
using VehiclesSearchService.ViewModels;

namespace VehiclesSearchService.Controllers
{
    /// <summary>
    /// :: TODO :: 
    /// - Add logging
    /// - Add caching using Redis
    /// - Add auth
    /// - Refactor methods (may methods should moved somewhere?)
    /// - Add pagination
    /// </summary>

    [Route("api/search/manufacturers")]
    [ApiController]
    public class ManufacturersController : ControllerBase
    {
        private readonly ILogger<VehiclesController> _logger;
        private readonly HttpClient _httpClient;
        private IOptions<NhtsaApiConfig> _nhtsaApiConfig;
        private readonly IVehiclesSearchSvc _searchSvc;
        private string _responseFormat = @"?format=json";

        public ManufacturersController(
            ILogger<VehiclesController> logger,
            IOptions<NhtsaApiConfig> nhtsaApiConfig,
            IVehiclesSearchSvc searchSvc)
        {
            _logger = logger;
            _nhtsaApiConfig = nhtsaApiConfig;
            
            _httpClient = new();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"application/json"));
            _httpClient.BaseAddress = new Uri($@"{_nhtsaApiConfig.Value.ApiUrl}/");
            
            _searchSvc = searchSvc;
        }

        private async Task<string> GetResponseAsync(string path)
        {
            var response = await _httpClient.GetAsync(path);

            return await response.Content.ReadAsStringAsync();
        }

        /// <summary>
        /// Searchs for the first non-empty/non-null property
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns>Endpoint uri</returns>
        private string GetFirstAppropriateProperty(MfrSearchViewModel searchCriteria)
        {
            var filledProperties = PropertyLookup.GetFilledProperties(searchCriteria);

            if (!filledProperties.Any())
                throw new InvalidParamsException();

            var containsNameOrId = filledProperties.Where(p =>
                (p.Name == nameof(MfrSearchViewModel.MfrName) ||
                p.Name == nameof(MfrSearchViewModel.MfrId)));

            // At first we're looking for MfrId or MfrName, because searching using these params are more efficient
            if (containsNameOrId.Any())
                return containsNameOrId.First().Name;

            switch (filledProperties.First().Name)
            {
                case nameof(MfrSearchViewModel.Country):
                    return nameof(MfrSearchViewModel.Country);

                case nameof(MfrSearchViewModel.City):
                    return nameof(MfrSearchViewModel.City);

                default:
                    throw new InvalidParamsException();
            }
        }

        private async Task<List<DetailedManufacturer>> GetDetailedManufacturers(string property, MfrSearchViewModel searchCriteria)
        {
            switch (property)
            {
                case nameof(MfrSearchViewModel.MfrName):
                    return await GetByIdentifiersAsync(searchCriteria.MfrName, searchCriteria);

                case nameof(MfrSearchViewModel.MfrId):
                    return await GetByIdentifiersAsync(searchCriteria!.MfrId!.ToString()!, searchCriteria);

                case nameof(MfrSearchViewModel.Country):
                case nameof(MfrSearchViewModel.City):
                case nameof(MfrSearchViewModel.State):
                    return await GetBySecondaryCreteriasAsync(searchCriteria);

                default:
                    throw new InvalidParamsException();
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetManufacturerInfoAsync([FromQuery] MfrSearchViewModel searchCriteria)
        {
            var appropriateProp = GetFirstAppropriateProperty(searchCriteria);

            List<DetailedManufacturer> manufacturers = await GetDetailedManufacturers(appropriateProp, searchCriteria);

            return Ok(manufacturers);
        }

        private async Task<List<DetailedManufacturer>> GetByIdentifiersAsync(string identifier, MfrSearchViewModel searchCreteria)
        {
            string uri = $@"GetManufacturerDetails/{identifier}{_responseFormat}";
            var response = await GetResponseAsync(uri);

            var filteredResult = _searchSvc.GetFilteredMfrs(response, searchCreteria);

            return filteredResult;
        }

        private async Task<List<DetailedManufacturer>> GetBySecondaryCreteriasAsync(MfrSearchViewModel searchCreteria)
        {
            string uri = $@"GetAllManufacturers{_responseFormat}";

            var manufacturers = QueueAllMfrsAsync(uri);

            List<DetailedManufacturer> filteredManufacturers = new();

            await foreach (var mfrs in manufacturers)
            foreach (var mfr in mfrs)
            {
                var newFilteredMfrs = await GetByIdentifiersAsync(mfr.MfrId.ToString(), searchCreteria);
                if(newFilteredMfrs.Any())
                    filteredManufacturers.AddRange(newFilteredMfrs);
            }

            return filteredManufacturers;
        }

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

            } while (!isLastEmpty && counter < 3);
        }
    }
}
