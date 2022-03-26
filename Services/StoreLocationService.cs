#nullable disable
using BlazorStoreFinder.Result;
using BlazorStoreFinder.Reverse;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
namespace BlazorStoreFinder
{
    public class StoreLocationService
    {
        private readonly BlazorStoreFinderContext _context;
        public StoreLocationService(BlazorStoreFinderContext context)
        {
            _context = context;
        }
        public async Task<List<StoreLocations>> GetStoreLocations()
        {
            return await _context.StoreLocations.OrderBy(x => x.Id).ToListAsync();
        }

        public async Task<StoreLocations> GetStoreLocation(int id)
        {
            return await _context.StoreLocations.FindAsync(id);
        }

        public async Task<StoreLocations> AddStoreLocation(StoreLocations storeLocation)
        {
            _context.StoreLocations.Add(storeLocation);
            await _context.SaveChangesAsync();
            return storeLocation;
        }

        public async Task DeleteStoreLocation(int id)
        {
            var storeLocation = await _context.StoreLocations.FindAsync(id);
            _context.StoreLocations.Remove(storeLocation);
            await _context.SaveChangesAsync();
        }

        public async Task<List<StoreSearchResult>> GetNearbyStoreLocations(Coordinate paramCoordinate)
        {
            List<StoreSearchResult> colStoreLocations = new List<StoreSearchResult>();

            var distanceInMeters = 40 * 1609.344; // 40 miles in meters
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var SearchLocation = geometryFactory.CreatePoint(paramCoordinate);

            colStoreLocations = await _context.StoreLocations
                .OrderBy(x => x.LocationData.Distance(SearchLocation))
                .Where(x => x.LocationData.IsWithinDistance(SearchLocation, distanceInMeters))
                .Select(x => new StoreSearchResult
                {
                    LocationName = x.LocationName,
                    LocationAddress = x.LocationAddress,
                    Distance = (x.LocationData.Distance(SearchLocation) / 1609.344)
                }).ToListAsync();

            return colStoreLocations;
        }

        // Geocode

        public async Task<Coordinate> GeocodeAddress(string address)
        {
            Coordinate coordinate = new Coordinate();

            // Create a HTTP Client to make the REST call

            // Search - Get Search Address
            // https://bit.ly/3JER1ii

            // Best practices for Azure Maps Search Service
            // https://bit.ly/3JFQkFt

            using (var client = new System.Net.Http.HttpClient())
            {
                // Get a Access Token from AuthService
                var AccessToken = await AuthService.GetAccessToken();

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Pass the Azure Maps Client Id
                client.DefaultRequestHeaders.Add("x-ms-client-id", AuthService.ClientId);
                // Pass the Access Token in the auth header
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

                // Build the URL
                StringBuilder sb = new StringBuilder();

                // Request a address search
                sb.Append("https://atlas.microsoft.com/search/address/json?");
                // Specify the api version and language
                sb.Append("&api-version=1.0&language=en-US");
                // Pass address
                sb.Append($"&query={address}");

                // Set the URL
                var url = new Uri(sb.ToString());

                // Call Azure maps and get the repsonse
                var Response = await client.GetAsync(url);

                // Read the response
                var responseContent = await Response.Content.ReadAsStringAsync();
                var AddressResult = JsonConvert.DeserializeObject<SearchAddressResult>(responseContent);

                // Create coordinate
                coordinate = new Coordinate(
                    Convert.ToDouble(AddressResult.results.FirstOrDefault()?.position.lon),
                    Convert.ToDouble(AddressResult.results.FirstOrDefault()?.position.lat));
            }

            return coordinate;
        }

        public async Task<SearchAddressResultReverse> GeocodeReverse(Coordinate paramCoordinate)
        {
            SearchAddressResultReverse result = new SearchAddressResultReverse();

            // Create a HTTP Client to make the REST call

            // Search - Get Search Address Reverse
            // https://bit.ly/3Nuz5cP
            using (var client = new System.Net.Http.HttpClient())
            {
                // Get a Access Token from AuthService
                var AccessToken = await AuthService.GetAccessToken();

                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Accept.Add(
                    new MediaTypeWithQualityHeaderValue("application/json"));

                // Pass the Azure Maps Client Id
                client.DefaultRequestHeaders.Add("x-ms-client-id", AuthService.ClientId);
                // Pass the Access Token in the auth header
                client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AccessToken);

                // Build the URL
                StringBuilder sb = new StringBuilder();

                // Request a address search
                sb.Append("https://atlas.microsoft.com/search/address/reverse/json?");
                // Specify the api version and language
                sb.Append("api-version=1.0");
                // Pass latitude
                sb.Append($"&query={paramCoordinate.X}");
                // Pass longitude 
                sb.Append($",{paramCoordinate.Y}");

                // Set the URL
                var url = new Uri(sb.ToString());

                // Call Azure maps and get the repsonse
                var Response = await client.GetAsync(url);

                // Read the response
                var responseContent = await Response.Content.ReadAsStringAsync();
                result = JsonConvert.DeserializeObject<SearchAddressResultReverse>(responseContent);
            }

            return result;
        }
    }
}