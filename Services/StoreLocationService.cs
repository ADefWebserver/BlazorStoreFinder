#nullable disable
using BlazorStoreFinder.Result;
using BlazorStoreFinder.Reverse;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;
using System.Data;
using System.Net.Http.Headers;
using System.Text;

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

        public List<StoreSearchResult> GetNearbyStoreLocations(Coordinate paramCoordinate)
        {
            List<StoreSearchResult> colStoreLocations = new List<StoreSearchResult>();

            // Using a raw SQL query because sometimes NetTopologySuite
            // cannot properly translate a query

            StringBuilder sb = new StringBuilder();

            // Set Distance to 25 miles
            sb.Append("declare @Distance as int = 25 ");
            // Declare the Coordinate
            sb.Append($"declare @Latitude as nvarchar(250) = '{paramCoordinate.Y}' ");
            sb.Append($"declare @Longitude as nvarchar(250) = '{paramCoordinate.X}' ");
            // Declare the Geography
            sb.Append("declare @location sys.geography ");
            sb.Append(" ");
            // Set the Geography to the Coordinate
            sb.Append("set @location = ");
            sb.Append("geography::STPointFromText('POINT(' + @Longitude + ' ' + @Latitude + ')', 4326) ");
            sb.Append(" ");
            // Search for Store Locations within the distance
            sb.Append("SELECT ");
            sb.Append("[LocationName], ");
            sb.Append("[LocationAddress], ");
            sb.Append("[LocationData].STDistance(@location) / 1609.3440000000001E0 AS [DistanceInMiles] ");
            sb.Append("FROM [StoreLocations] ");
            sb.Append("where [LocationData].STDistance(@location) / 1609.3440000000001E0 < @Distance ");
            sb.Append("order by [LocationData].STDistance(@location) / 1609.3440000000001E0 ");

            using (SqlConnection connection =
            new SqlConnection(_context.Database.GetConnectionString()))
            {
                SqlCommand command = new SqlCommand(sb.ToString(), connection);
                connection.Open();
                SqlDataReader reader = command.ExecuteReader();
                
                while (reader.Read())
                {
                    colStoreLocations.Add(new StoreSearchResult
                    {
                        LocationName = reader["LocationName"].ToString(),
                        LocationAddress = reader["LocationAddress"].ToString(),
                        Distance = double.Parse(reader["DistanceInMiles"].ToString())
                    });
                }       
                
                reader.Close();
            }

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