using System;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
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
        const string apiUrl = "http://10.11.12.13:5010/identity";
        private static TokenResponse _token = null;


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
            Console.WriteLine("3: Get Resource Owner reference token");
            Console.WriteLine("4: Print stored token");
            Console.WriteLine("5: Decode JWT");
            Console.WriteLine("6: Print claims with IntrospectionClient (via ApiResources)");
            Console.WriteLine("7: Print claims with UserInfoClient (via IdentityResources)");
            Console.WriteLine("8: Web API - Test existing token");
            Console.WriteLine("9: Web API - Test existing token with UserInfoClient");
            Console.WriteLine("0: Exit");
            Console.WriteLine("===========================");
            Console.WriteLine();

            var key = Console.ReadKey(true);

            switch (key.KeyChar.ToString())
            {
                case "1":
                    await GetTokenAsync(scopes: "ConsoleApi");
                    break;

                case "2":
                    await GetRoTokenAsync(clientId: "ro.client", scopes: "api1 openid");
                    break;

                case "3":
                    await GetRoTokenAsync(clientId: "ro.client.reference", scopes: "api1 openid"); // offline_access
                    break;

                case "4":
                    if (_token == null)
                    {
                        Console.WriteLine("Token store empty");
                    }
                    else
                    {
                        Console.WriteLine("TokenType: {0}", _token.TokenType);
                        Console.WriteLine("AccessToken: {0}", _token.AccessToken);
                        Console.WriteLine("RefreshToken: {0}", _token.RefreshToken);
                        Console.WriteLine("IdentityToken: {0}", _token.IdentityToken);
                    }
                    break;

                case "5":
                    DecodeJwtToken();
                    break;

                case "6":
                    await PrintClaimsIntrospectionClient();
                    break;

                case "7":
                    await PrintClaimsUserInfoClient();
                    break;

                case "8":
                    await UseTokenWithWebApiAsync(action: "GetUserClaims");
                    break;
                    
                case "9":
                    await UseTokenWithWebApiAsync(action: "GetUserClaimsDisco");
                    break;

                case "0":
                    return;
                default:
                    break;
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
        private static async Task UseTokenWithWebApiAsync(string action)
        {
            string accessToken = String.Empty; ;

            if (_token == null)
            {
                Console.WriteLine("No token in store");
            }
            else
            {
                Console.WriteLine("Enter token or press enter to use existing: ");
                var readLineToken = Console.ReadLine();
                accessToken = string.IsNullOrEmpty(readLineToken) ? _token.AccessToken : readLineToken;
            }

            if (!string.IsNullOrEmpty(accessToken))
            {
                var client = new HttpClient();
                client.SetBearerToken(accessToken);

                // Get response from Web API
                var response = await client.GetAsync(requestUri: $"{apiUrl}/{action}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine("Request returned statuscode {0}", response.StatusCode);
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine(JsonConvert.DeserializeObject(content));
                }
            }
            else
            {
                Console.WriteLine("No token available");
            }

        }

        private static async Task GetTokenAsync(string scopes)
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
            var tokenResponse = await tokenClient.RequestClientCredentialsAsync(scopes);

            if (tokenResponse.IsError)
            {
                Console.WriteLine("Token endpoint error: {0}", tokenResponse.Error);
                return;
            }

            Console.WriteLine("We got a token!");

            // Lets store it in the local token member
            _token = tokenResponse;
        }

        private static async Task GetRoTokenAsync(string clientId, string scopes)
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
            var tokenClient = new TokenClient(address: disco.TokenEndpoint, clientId: clientId, clientSecret: clientSecret);
            var tokenResponse = await tokenClient.RequestResourceOwnerPasswordAsync(
                userName: "alice", password: "pass", scope: scopes); // api1 openid profile
            // scope here must somehow match what is in the identity resources of ids4

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return;
            }


            // Write the token type
            Console.WriteLine("Logged on using the {0}", clientId);
            Console.WriteLine("token_type: {0}", tokenResponse.Json.Value<string>("token_type"));
            Console.WriteLine("Token length {0}{1}", tokenResponse.AccessToken.Length, Environment.NewLine);
            // store accesstoken
            _token = tokenResponse;
        }

        private static async Task PrintClaimsIntrospectionClient()
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


            var introspectionClient = new IntrospectionClient(endpoint: disco.IntrospectionEndpoint, clientId: "api1", clientSecret: clientSecret);
            var response = await introspectionClient.SendAsync(new IntrospectionRequest { Token = _token.AccessToken });

            if (!response.IsError)
            {
                Console.WriteLine("Claims for the user");
                response.Claims.ToList().ForEach(claim => Console.WriteLine("{0}: {1}", claim.Type, claim.Value));
                Console.WriteLine("\n\n");
            }
        }

        private static async Task PrintClaimsUserInfoClient()
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

            // UserInfoClient for claims using IdentityModel
            var userInfo = new UserInfoClient(endpoint: disco.UserInfoEndpoint);
            var userInfoResponse = await userInfo.GetAsync(token: _token.AccessToken);

            if (!userInfoResponse.IsError)
            {
                Console.WriteLine("Claims for the user");
                userInfoResponse.Claims.ToList().ForEach(claim => Console.WriteLine("{0}: {1}", claim.Type, claim.Value));
                Console.WriteLine("\n\n");
            }
        }

        private static void DecodeJwtToken()
        {
            if (_token == null)
            {
                Console.WriteLine("No token in store");
                return;
            }

            try
            {
                var handler = new JwtSecurityTokenHandler();
                if (handler.CanReadToken(token: _token.AccessToken))
                {
                    JwtSecurityToken jwt = handler.ReadJwtToken(token: _token.AccessToken);
                    jwt.Claims.ToList().ForEach(claim => Console.WriteLine("{0}: {1}", claim.Type, claim.Value));
                }
                else
                {
                    Console.WriteLine("Malformed JWT Token");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

    }
}
