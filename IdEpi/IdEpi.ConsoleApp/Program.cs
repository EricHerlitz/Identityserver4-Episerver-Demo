using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using IdentityModel.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace IdEpi.ConsoleApp
{
    class Program
    {
        const string authority = "http://10.11.12.13:5000";
        const string clientSecret = "secret";
        const string scope = "api1";
        const string apiUrl = "http://10.11.12.13:5010/identity";
        private static string _token = null;


        static void Main(string[] args) => MainAsync().GetAwaiter().GetResult();

        public static async Task MainAsync()
        {
            ConfigureConsole();
            await Run();
        }

        private static async Task Run()
        {
            Console.WriteLine("===========================");
            Console.WriteLine("1: Get Token");
            Console.WriteLine("2: Get Resource Owner token");
            Console.WriteLine("3: Use existing token with Web API");
            Console.WriteLine("4: Print stored token");
            Console.WriteLine("9: Exit");
            Console.WriteLine("===========================");
            Console.WriteLine();

            var key = Console.ReadKey(true);

            switch (key.KeyChar.ToString())
            {
                case "1":
                    await GetTokenAsync();
                    break;

                case "2":
                    await GetRoTokenAsync();
                    break;

                case "3":
                    await UseTokenWithWebApiAsync();
                    break;

                case "4":
                    Console.WriteLine(string.IsNullOrEmpty(_token) ? "No token in store" : _token);
                    break;

                case "9":
                default:
                    return;
            }

            Console.WriteLine();
            await Run();
        }

        private static void ConfigureConsole()
        {
            Console.Title = $"Console Client {Process.GetCurrentProcess().Id}";
            Console.SetIn(new StreamReader(Console.OpenStandardInput(), Console.InputEncoding, false, bufferSize: 2048));
        }

        /// <summary>
        /// Use existing token with Web API
        /// This uses a HttpClient
        /// </summary>
        /// <returns></returns>
        private static async Task UseTokenWithWebApiAsync()
        {
            if (string.IsNullOrEmpty(_token))
            {
                Console.WriteLine("Enter token: ");
                _token = Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Enter token or press enter to use existing: ");
                var readLineToken = Console.ReadLine();
                _token = string.IsNullOrEmpty(readLineToken) ? _token : readLineToken;
            }

            if (!string.IsNullOrEmpty(_token))
            {
                var client = new HttpClient();
                client.SetBearerToken(_token);

                // Get response from Web API
                var response = await client.GetAsync(apiUrl);

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("response.StatusCode");
                    Console.WriteLine(response.StatusCode);
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(JsonConvert.DeserializeObject(content));
                }
            }
            else
            {
                Console.WriteLine("No token entered");
            }

        }


        private static async Task GetTokenAsync()
        {
            // Get DiscoveryClient from IdentityServer using IdentityModel
            DiscoveryClient discoInstance = new DiscoveryClient(authority: authority)
            {
                Policy = new DiscoveryPolicy { RequireHttps = false } // For development
            };

            DiscoveryResponse disco = await discoInstance.GetAsync();

            if (disco.IsError)
            {
                Console.WriteLine("Disco error {0}", disco.Error);
                return;
            }

            // TokenClient from IdentityModel
            var tokenClient = new TokenClient(address: disco.TokenEndpoint, clientId: "client", clientSecret: clientSecret);
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync(scope: scope);

            if (tokenResponse.IsError)
            {
                Console.WriteLine("Token endpoint error: {0}", tokenResponse.Error);
                return;
            }

            Console.WriteLine("We got a token!");

            // Lets store it in the local token member
            _token = tokenResponse.AccessToken;

            return;
            // Call api
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
            // Get DiscoveryClient from IdentityServer using IdentityModel
            DiscoveryClient discoInstance = new DiscoveryClient(authority: authority)
            {
                Policy = new DiscoveryPolicy { RequireHttps = false } // For development
            };

            DiscoveryResponse disco = await discoInstance.GetAsync();

            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return;
            }

            // TokenClient from IdentityModel
            var tokenClient = new TokenClient(address: disco.TokenEndpoint, clientId: "ro.client", clientSecret: clientSecret);
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(
                userName: "alice", password: "pass", scope: "api1 openid profile");

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }


            // Write the token type
            Console.WriteLine("token_type: {0}{1}", tokenResponse.Json.Value<string>("token_type"), Environment.NewLine);

            // store accesstoken
            _token = tokenResponse.AccessToken;

            // UserInfoClient for claims using IdentityModel
            var userInfo = new UserInfoClient(endpoint: disco.UserInfoEndpoint);
            var userInfoResponse = await userInfo.GetAsync(token: tokenResponse.AccessToken);

            if (!userInfoResponse.IsError)
            {
                Console.WriteLine("Claims for the user");
                userInfoResponse.Claims.ToList().ForEach(claim => Console.WriteLine("{0}: {1}", claim.Type, claim.Value));
                Console.WriteLine("\n\n");
            }

            return;

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
                Console.WriteLine(content);

                var a = JsonConvert.DeserializeObject<object>(content);
                Console.WriteLine(a);
                //Console.WriteLine(JArray.Parse(content));
            }
        }
    }
}
