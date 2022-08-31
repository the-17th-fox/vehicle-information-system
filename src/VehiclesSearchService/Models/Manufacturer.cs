using Newtonsoft.Json;

namespace VehiclesSearchService.Models
{
    public class Manufacturer
    {
        [JsonProperty(PropertyName = "Mfr_ID")]
        public int MfrId { get; set; }

        [JsonProperty(PropertyName = "Mfr_Name")]
        public string MfrFullName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "Mfr_CommonName")]
        public string MfrCommonName { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "StateProvince")]
        public string State { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public List<VehicleType> VehicleTypes { get; set; } = new();
    }

    public class VehicleType
    {
        [JsonProperty(PropertyName = "GVWRFrom")]
        public string WeightRatingFrom { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "GVWRTo")]
        public string WeightRatingTo { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
