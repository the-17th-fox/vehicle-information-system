using Newtonsoft.Json;
using System.Text.Json.Serialization;

namespace VehiclesSearchService.Models
{
    // Used for JSON deserialization
    public class MfrDetailsResponse<TManufacturerType> 
        where TManufacturerType : BaseManufacturer
    {
        public List<TManufacturerType> Results { get; set; } = new();

        public static bool TryDeserializeResponse(string response, out List<TManufacturerType>? result)
        {
            var options = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            var deserializedResp = JsonConvert.DeserializeObject<MfrDetailsResponse<TManufacturerType>>(response, options);

            result = deserializedResp.Results;

            return result is not null ? true : false;
        }
    }

}
