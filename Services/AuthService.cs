using Microsoft.Identity.Client;
using Microsoft.JSInterop;

namespace BlazorStoreFinder
{
    public static class AuthService
    {
        private const string AuthorityFormat = 
            "https://login.microsoftonline.com/{0}/oauth2/v2.0";
        
        private const string MSGraphScope = 
            "https://atlas.microsoft.com/.default";
        
        public static string? ClientId;
        public static string? AadTenant;
        public static string? AadAppId;
        public static string? AppKey;

        internal static void SetAuthSettings(IConfigurationSection AzureMaps)
        {
            // A call in Program.cs to this method is made to set the values
            ClientId = AzureMaps.GetValue<string>("ClientId");
            AadTenant = AzureMaps.GetValue<string>("AadTenant");
            AadAppId = AzureMaps.GetValue<string>("AadAppId");
            AppKey = AzureMaps.GetValue<string>("AppKey");
        }

        [JSInvokable]
        public static async Task<string> GetAccessToken()
        {
            // Create a confidential client
            IConfidentialClientApplication daemonClient;

            // Create a builder for the confidential client
            daemonClient = ConfidentialClientApplicationBuilder
                .Create(AadAppId)
                .WithAuthority(string.Format(AuthorityFormat, AadTenant))
                .WithClientSecret(AppKey)
                .Build();

            // Get the access token for the confidential client
            AuthenticationResult authResult =
            await daemonClient.AcquireTokenForClient(new[] { MSGraphScope })
            .ExecuteAsync();

            // Return the access token
            return authResult.AccessToken;
        }
    }
}