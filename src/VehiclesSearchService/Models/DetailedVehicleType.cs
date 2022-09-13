using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json;

namespace VehiclesSearchService.Models
{
    public class DetailedVehicleType : BaseVehicleType
    {
        [JsonProperty(PropertyName = "GVWRFrom")]
        public string WeightRatingFrom { get; set; } = string.Empty;
        [JsonProperty(PropertyName = "GVWRTo")]
        public string WeightRatingTo { get; set; } = string.Empty;
    }
}
