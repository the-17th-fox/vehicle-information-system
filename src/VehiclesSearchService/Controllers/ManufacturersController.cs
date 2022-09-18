using Common.CustomExceptions;
using Common.Extensions;
using Common.ViewModels;
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
    /// - Add caching using Redis
    /// - Add auth
    /// - Add pagination
    /// </summary>

    [Route("api/search/manufacturers")]
    [ApiController]
    public class ManufacturersController : ControllerBase
    {
        private readonly ILogger<ManufacturersController> _logger;
        private readonly IManufacturersSearchSvc _searchSvc;

        public ManufacturersController(
            ILogger<ManufacturersController> logger,
            IManufacturersSearchSvc searchSvc)
        {
            _logger = logger;
            _searchSvc = searchSvc;
        }

        [HttpGet]
        public async Task<IActionResult> GetManufacturersInfoAsync([FromQuery] MfrSearchViewModel searchCriteria)
        {
            _logger.LogInformation(LogEventType.MfrsInfoRequest, searchCriteria.FilledPropertiesValues());

            var manufacturers = await _searchSvc.GetDetailedMfrsAsync(searchCriteria);

            _logger.LogInformation(LogEventType.MfrsInfoRequestSucceded, searchCriteria.FilledPropertiesValues());
            return Ok(manufacturers);
        }
    }
}
