using System.Text.Json.Serialization;

namespace VehiclesSearchService.Models
{
    // Used for JSON deserialization
    public class ManufacturerDetailsResponse
    {
        public int Count { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SearchCriteria { get; set; } = string.Empty;
        public List<Manufacturer> Results { get; set; } = new();
    }
}
