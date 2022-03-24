#nullable disable
namespace BlazorStoreFinder.Reverse
{
    public class SearchAddressResultReverse
    {
        public Summary summary { get; set; }
        public Address[] addresses { get; set; }
    }

    public class Summary
    {
        public int queryTime { get; set; }
        public int numResults { get; set; }
    }

    public class Address
    {
        public Address1 address { get; set; }
        public string position { get; set; }
    }

    public class Address1
    {
        public string streetNumber { get; set; }
        public object[] routeNumbers { get; set; }
        public string streetName { get; set; }
        public string streetNameAndNumber { get; set; }
        public string countryCode { get; set; }
        public string countrySubdivision { get; set; }
        public string countrySubdivisionName { get; set; }
        public string municipality { get; set; }
        public string postalCode { get; set; }
        public string country { get; set; }
        public string countryCodeISO3 { get; set; }
        public string freeformAddress { get; set; }
        public string extendedPostalCode { get; set; }
    }

}
