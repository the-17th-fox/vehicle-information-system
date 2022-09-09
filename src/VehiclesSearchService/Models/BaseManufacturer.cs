using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace VehiclesSearchService.Models
{
    public class BaseManufacturer
    {
        [JsonProperty(PropertyName = "Mfr_ID")]
        public int MfrId { get; set; }

        [JsonProperty(PropertyName = "Mfr_Name")]
        public string MfrFullName { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "Mfr_CommonName")]
        public string MfrCommonName { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;
        public List<BaseVehicleType> VehicleTypes { get; set; } = new();
    }
}
