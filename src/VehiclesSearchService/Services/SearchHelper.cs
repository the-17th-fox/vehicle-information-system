using Common.CustomExceptions;
using Common.Extensions;
using Common.Utilities;
using Newtonsoft.Json;
using VehiclesSearchService.Models;
using VehiclesSearchService.Utilities;
using VehiclesSearchService.ViewModels;

namespace VehiclesSearchService.Services
{
    public class SearchHelper
    {
        public static bool TryDeserializeMfrs<TManufacturerType>(string json, out List<TManufacturerType>? mfrs)
        {
            var options = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            mfrs = JsonConvert.DeserializeObject<List<TManufacturerType>>(json);

            return mfrs is not null ? true : false;
        }

        public static bool TrySerializeMfrs(object mfrs, out string result)
        {
            var options = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore,
            };

            result = JsonConvert.SerializeObject(mfrs);

            return result is not null ? true : false;
        }

        /// <summary>
        /// Searchs for the first non-empty/non-null property
        /// </summary>
        /// <param name="searchCriteria"></param>
        /// <returns<b>>First filled property<b> of provided model or <b>empty string</b></returns>
        public static string GetFirstAppropriateProperty(MfrSearchViewModel searchCriteria)
        {
            var filledProperties = PropertyLookup.GetFilledProperties(searchCriteria);

            if (!filledProperties.Any())
                throw new InvalidParamsException(LogEventType.ParameterMissed.Replace("argument", searchCriteria.AllPropertiesValues()));

            var containsNameOrId = filledProperties.Where(p =>
                (p.Name == nameof(MfrSearchViewModel.MfrName) ||
                p.Name == nameof(MfrSearchViewModel.MfrId)));

            // At first we're looking for MfrId or MfrName, because searching using these params are more efficient
            if (containsNameOrId.Any())
                return containsNameOrId.First().Name;

            foreach (var prop in filledProperties)
            {
                switch (prop.Name)
                {
                    case nameof(MfrSearchViewModel.Country):
                        return nameof(MfrSearchViewModel.Country);

                    case nameof(MfrSearchViewModel.City):
                        return nameof(MfrSearchViewModel.City);

                    default:
                        break;
                }
            }

            return string.Empty;
        }
    }
}
