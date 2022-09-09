using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace VehiclesSearchService.Models
{
    // Used for JSON deserialization
    public class MfrDetailsResponse<TManufacturerType, TVehicleType> 
        where TManufacturerType : BaseManufacturer
        where TVehicleType : BaseVehicleType
    {
        public int Count { get; set; }
        public string Message { get; set; } = string.Empty;
        public string SearchCriteria { get; set; } = string.Empty;
        public List<TManufacturerType> Results { get; set; } = new();

        public static bool TryParseResponse(string response, out MfrDetailsResponse<TManufacturerType, TVehicleType>? result)
        {
            var options = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            result = JsonConvert.DeserializeObject<MfrDetailsResponse<TManufacturerType, TVehicleType>>(response, options);

            return result is not null ? true : false;
        }

        public static MfrDetailsResponse<TManufacturerType, TVehicleType>? TryParseResponse(string response)
        {
            var options = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            var result = JsonConvert.DeserializeObject<MfrDetailsResponse<TManufacturerType, TVehicleType>>(response, options);

            return result;
        }
    }

}
