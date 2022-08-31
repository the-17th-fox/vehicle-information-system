using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using VehiclesSearchService.Models;
using VehiclesSearchService.Services;
using VehiclesSearchService.Utilities;
using VehiclesSearchService.ViewModels;

namespace VehiclesSearchService.Controllers
{
    [Route("api/search/manufacturers")]
    [ApiController]
    public class ManufacturersController : ControllerBase
    {
        private readonly ILogger<VehiclesController> _logger;
        private readonly HttpClient _httpClient;
        private IOptions<NhtsaApiConfig> _nhtsaApiConfig;
        private readonly IVehiclesSearchSvc _searchSvc;
        private string _apiUrl { get => _nhtsaApiConfig.Value.ApiUrl; }

        public ManufacturersController(
            ILogger<VehiclesController> logger,
            IOptions<NhtsaApiConfig> nhtsaApiConfig,
            IVehiclesSearchSvc searchSvc)
        {
            _logger = logger;
            _nhtsaApiConfig = nhtsaApiConfig;
            _httpClient = new();
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue(@"application/json"));
            _searchSvc = searchSvc;
        }

        private async Task<string> GetResponse(string uri)
        {
            _httpClient.BaseAddress = new Uri(uri);

            var response = await _httpClient.GetAsync(_httpClient.BaseAddress);

            return await response.Content.ReadAsStringAsync();
        }


        // Problem :: Users still have to provide mfrId or mfrName
        // Goal :: Users should be able to find manufacturers by any parameter they want
        [HttpGet]
        public async Task<IActionResult> GetMfrInfo([FromQuery] ManufacturerSearchViewModel searchCriteria)
        {
            if (searchCriteria.MfrId is null && string.IsNullOrWhiteSpace(searchCriteria.MfrName))
                throw new Exception();

            var requestParam = searchCriteria.MfrId is null ? searchCriteria.MfrName : searchCriteria.MfrId.ToString();

            string mfrDetails = await GetResponse(@$"{_apiUrl}/getmanufacturerdetails/{requestParam}?format=json");

            List<Manufacturer>? result;
            if ((result = _searchSvc.ProvideManufacturersCheckout(mfrDetails, searchCriteria)).Count == 0)
            {
                return NotFound();
            }

            return Ok(result);
        }
    }
}
