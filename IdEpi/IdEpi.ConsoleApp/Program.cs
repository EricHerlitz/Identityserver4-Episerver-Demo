using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json.Linq;

namespace IdEpi.ConsoleApp
{
    class Program
    {
        const string authority = "http://localhost:5000";
        const string clientSecret = "secret";
        const string scope = "api1";
        const string apiUrl = "http://localhost:5010/identity";

        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            Console.Title = "Console Client";

            //await GetTokenAsync();

            await GetRoTokenAsync();

        }

        private static async Task GetTokenAsync()
        {
            var disco = await DiscoveryClient.GetAsync(authority);

            if (disco.IsError)
            {
                Console.WriteLine("Disco error {0}", disco.Error);
                return;
            }

            var tokenClient = new TokenClient(disco.TokenEndpoint, "client", clientSecret);
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync(scope);

            if (tokenResponse.IsError)
            {
                Console.WriteLine("Token endpoint error: {0}", tokenResponse.Error);
                return;
            }

            Console.WriteLine("We got a token!");

            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync(apiUrl);
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


        private static async Task GetRoTokenAsync()
        {
            // discover endpoints from metadata
            var disco = await DiscoveryClient.GetAsync(authority);
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // request token
            var tokenClient = new TokenClient(disco.TokenEndpoint, "ro.client", clientSecret);
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync("alice", "pass", "api1 openid profile");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }
                
            Console.WriteLine("tokenResponse.Json");
            Console.WriteLine(tokenResponse.Json);
            Console.WriteLine("\n\n");

            // userinfo for claims
            var userInfo = new UserInfoClient(disco.UserInfoEndpoint);
            var userInfoResponse = await userInfo.GetAsync(tokenResponse.AccessToken);

            if (!userInfoResponse.IsError)
            {
                Console.WriteLine("Claims for the user");
                userInfoResponse.Claims.ToList().ForEach(claim => Console.WriteLine("{0}: {1}", claim.Type, claim.Value));
                Console.WriteLine("\n\n");
            }


            // call api
            var client = new HttpClient();
            client.SetBearerToken(tokenResponse.AccessToken);

            var response = await client.GetAsync(apiUrl);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("response.StatusCode");
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("API response.Content");
                Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
