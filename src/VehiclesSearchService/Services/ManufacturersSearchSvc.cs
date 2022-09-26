using Common.CustomExceptions;
using Common.Extensions;
using Common.ViewModels;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Reflection;
using VehiclesSearchService.Controllers;
using VehiclesSearchService.Infrastructure;
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
        private readonly ICacheRepository _cacheRep;
        private readonly IOptions<RedisCacheConfig> _cacheConfig;

        public ManufacturersSearchSvc(IOptions<NhtsaApiConfig> nhtsaApiConfig, 
            ILogger<ManufacturersSearchSvc> logger,
            ICacheRepository cacheRep,
            IOptions<RedisCacheConfig> cacheConfig)
        {
            _nhtsaApiConfig = nhtsaApiConfig;

            _httpClient = new();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(_nhtsaApiConfig.Value.RequestMediaType));
            _httpClient.BaseAddress = new Uri($@"{_nhtsaApiConfig.Value.ApiUrl}/");

            _logger = logger;

            _cacheRep = cacheRep;
            _cacheConfig = cacheConfig;
        }

        public async Task<List<DetailedManufacturer>> GetDetailedMfrsAsync(MfrSearchViewModel searchCriteria)
        {
            var cacheKeyName = $"Mfr/FastSearch:{searchCriteria.FilledPropertiesValues()}";
            List<DetailedManufacturer>? manufacturers;

            manufacturers = await TryGetFromCacheAsync<DetailedManufacturer>(cacheKeyName);
            if (manufacturers is not null && manufacturers.Count > 0)
            {
                _logger.LogInformation(LogEventType.CacheRetrieved, cacheKeyName);
                return manufacturers;
            }

            var appropriateProp = SearchHelper.GetFirstAppropriateProperty(searchCriteria);

            switch (appropriateProp)
            {
                case nameof(MfrSearchViewModel.MfrName):
                    manufacturers = await GetByIdentifierAsync(searchCriteria.MfrName, searchCriteria); break;

                case nameof(MfrSearchViewModel.MfrId):
                    manufacturers = await GetByIdentifierAsync(searchCriteria!.MfrId!.ToString()!, searchCriteria); break;

                case nameof(MfrSearchViewModel.Country):
                case nameof(MfrSearchViewModel.City):
                case nameof(MfrSearchViewModel.State):
                    manufacturers = await GetBySecondaryCreteriasAsync(searchCriteria); break;

                default:
                    throw new Exception(LogEventType.MfrsInfoRequestFailed.Replace("params", searchCriteria.AllPropertiesValues()));
            }

            await UpdateCacheAsync<DetailedManufacturer>(manufacturers, cacheKeyName, _cacheConfig.Value.CacheExpirationHours);
            return manufacturers;
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

        private static List<DetailedManufacturer> GetFilteredMfrs(List<DetailedManufacturer> manufacturers, MfrSearchViewModel searchCriteria)
        {
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

        // Request methods section below
        private async Task<string> GetResponseAsync(string path, string cacheKeyName)
        {
            string response = string.Empty;

            response = await _cacheRep.GetAsync(cacheKeyName);
            if(!string.IsNullOrWhiteSpace(response))
            {
                _logger.LogInformation(LogEventType.CacheRetrieved, cacheKeyName);
                return response;
            }

            _logger.LogInformation(LogEventType.CacheNotFound, cacheKeyName);

            var getTask = await _httpClient.GetAsync(path);
            response = await getTask.Content.ReadAsStringAsync();
            await _cacheRep.UpdateCacheAsync(cacheKeyName, response, _cacheConfig.Value.CacheExpirationHours);
            
            return response;
        }

        private async Task<List<DetailedManufacturer>> GetByIdentifierAsync(string identifier, MfrSearchViewModel searchCreteria)
        {
            string uri = $@"GetManufacturerDetails/{identifier}{_nhtsaApiConfig.Value.RequestPathFormating}";
            string cacheKeyName = $"Mfr/GetByIdentifier:{identifier}";

            string response = await GetResponseAsync(uri, cacheKeyName);

            List<DetailedManufacturer>? manufacturers;
            MfrDetailsResponse<DetailedManufacturer>.TryDeserializeResponse(response, out manufacturers);

            var filteredResult = GetFilteredMfrs(manufacturers ??= new(), searchCreteria);

            return filteredResult;
        }

        private async Task<List<DetailedManufacturer>> GetBySecondaryCreteriasAsync(MfrSearchViewModel searchCreteria)
        {
            List<DetailedManufacturer> filteredResult = new();
            var pagedMfrs = QueueAllMfrsAsync();

            await foreach (var mfrs in pagedMfrs)
            foreach (var mfr in mfrs)
            {
                var newFilteredMfrs = await GetByIdentifierAsync(mfr.MfrId.ToString(), searchCreteria);
                if (newFilteredMfrs.Any())
                    filteredResult.AddRange(newFilteredMfrs);
            }

            return filteredResult;
        }

        private async IAsyncEnumerable<List<BaseManufacturer>> QueueAllMfrsAsync()
        {
            bool isLastEmpty = false;
            int page = 1;
            string cacheKeyName = $"Mfr/GetPage:{page}";
            string uri = $@"GetAllManufacturers{_nhtsaApiConfig.Value.RequestPathFormating}&page={page++}";
            var newMfrs = new List<BaseManufacturer>();

            do
            {
                string response = await GetResponseAsync(uri, cacheKeyName);
                MfrDetailsResponse<BaseManufacturer>.TryDeserializeResponse(response, out newMfrs);

                if(newMfrs!.Count > 0)
                {
                    yield return newMfrs;
                }

                isLastEmpty = true;
            } 
            while (!isLastEmpty);
        }
        // End of request methods section 

        public async Task<List<TManufacturerType>> TryGetFromCacheAsync<TManufacturerType>(string cacheKeyName)
            where TManufacturerType : BaseManufacturer
        {
            List<TManufacturerType> mfrs = new();

            string response = await _cacheRep.GetAsync(cacheKeyName);
            
            if (string.IsNullOrWhiteSpace(response))
            {
                _logger.LogInformation(LogEventType.CacheNotFound, cacheKeyName);
                return mfrs;
            }

            if (!SearchHelper.TryDeserializeMfrs<TManufacturerType>(response, out mfrs!))
                throw new Exception();

            return mfrs;
        }

        public async Task UpdateCacheAsync<TManufacturerType>(List<TManufacturerType> mfrs, string cacheKeyName, double expireAfter)
            where TManufacturerType : BaseManufacturer
        {
            var json = string.Empty;
            if (!SearchHelper.TrySerializeMfrs(mfrs, out json))
                throw new Exception();

            await _cacheRep.UpdateCacheAsync(cacheKeyName, json, _cacheConfig.Value.CacheExpirationHours);
        }
    }
}
