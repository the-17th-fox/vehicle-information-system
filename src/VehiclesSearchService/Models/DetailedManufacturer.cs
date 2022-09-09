using Newtonsoft.Json;

namespace VehiclesSearchService.Models
{
    public class DetailedManufacturer : BaseManufacturer
    {
        public string City { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "StateProvince")]
        public string State { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
    }

    public class DetailedVehicleType : BaseVehicleType
    {
        [JsonProperty(PropertyName = "GVWRFrom")]
        public string WeightRatingFrom { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "GVWRTo")]
        public string WeightRatingTo { get; set; } = string.Empty;
    }
}
