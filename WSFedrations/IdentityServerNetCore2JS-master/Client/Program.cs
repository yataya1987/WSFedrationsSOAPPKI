using IdentityModel.Client;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace Client
{
    public class Program
    {
        public static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        private static async Task MainAsync()
        {
           

            // FIRST PART REQUEST TOKEN USING TEST USERS 

            var disco = await DiscoveryClient.GetAsync("http://localhost:5000");
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            var tokenClientRo = new TokenClient(disco.TokenEndpoint, "ro.client", "secret");
            var tokenResponRo = await tokenClientRo.RequestResourceOwnerPasswordAsync("diego", "diegopassword", "DM_secure_API");

            if (tokenResponRo.IsError)
            {
                Console.WriteLine(tokenResponRo.Error);
                return;
            }
            Console.WriteLine(tokenResponRo.Json);
            Console.WriteLine("\n\n");


            
            // SECOND PART REQUEST TOKEN USING CLIENT CREDENTIALS 
            var tokenClient = new TokenClient(disco.TokenEndpoint, "diegomary", "dmpassword");
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync("DM_secure_API");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }

            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync("http://localhost:5001/api/identity");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
