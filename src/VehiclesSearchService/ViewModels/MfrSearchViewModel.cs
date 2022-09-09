namespace VehiclesSearchService.ViewModels
{
    public class MfrSearchViewModel
    {
        public int? MfrId { get; set; } = null;
        public string MfrName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
    }
}
