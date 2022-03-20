#nullable disable
namespace BlazorStoreFinder
{
    public partial class StoreLocations
    {
        public int Id { get; set; }
        public string LocationName { get; set; }
        public string LocationLatitude { get; set; }
        public string LocationLongitude { get; set; }
        public string LocationAddress { get; set; }
        public NetTopologySuite.Geometries.Point LocationData { get; set; }
    }
}