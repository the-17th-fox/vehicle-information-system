using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace VehiclesSearchService.Models
{
    public class BaseVehicleType
    {
        public bool IsPrimary { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
