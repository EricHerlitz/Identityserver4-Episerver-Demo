using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using IdentityModel.Client;

namespace IdEpi.WebClient.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Secure()
        {
            ViewData["Message"] = "Secure page.";

            //Console.WriteLine("----------------------------");
            //Console.WriteLine("User Claims");
            //User.Claims.ToList().ForEach(claim => Console.WriteLine("{0} {1}", claim.Type, claim.Value));


            //var client = new HttpClient();
            //client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


            // UserInfo request
            //// Get DiscoveryClient from IdentityServer using IdentityModel
            //DiscoveryClient discoInstance = new DiscoveryClient(authority: Startup.Authority)
            //{
            //    Policy = new DiscoveryPolicy { RequireHttps = false } // For development
            //};
            //DiscoveryResponse disco = await discoInstance.GetAsync();
            //var userInfo = new UserInfoClient(disco.UserInfoEndpoint);
            //var accessToken = await HttpContext.GetTokenAsync(tokenName: "access_token");
            //var userInfoResponse = await userInfo.GetAsync(accessToken);
            //Console.WriteLine("userInfoResponse Claims");
            //userInfoResponse.Claims.ToList().ForEach(claim => Console.WriteLine("{0} {1}", claim.Type, claim.Value));
            //Console.WriteLine("----------------------------");

            return View();
        }

        /// <summary>
        /// This require IdentityModel to be implemented in the project
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public async Task<IActionResult> Api()
        {
            ViewData["Message"] = "Api page.";

            //AuthenticationHttpContextExtensions.GetTokenAsync()
            //var a = AuthenticationTokenExtensions.GetTokenValue();
            var accessToken = await HttpContext.GetTokenAsync(tokenName: "access_token");

            if (accessToken == null)
            {
                Console.WriteLine("Fel token");
            }
            else
            {
                Console.WriteLine("accessToken");
                Console.WriteLine(accessToken);
            }


            //IClaimsPrincipal icp = Thread.CurrentPrincipal as IClaimsPrincipal;

            // access_token
            //var context = ControllerContext.HttpContext;
            //var authHeader = context.Request.Headers["Authorization"];
            //var token = Request.Cookies["idsrv"];

            // call api
            var client = new HttpClient();
            //client.SetBearerToken(accessToken); // if IdentityModel is installed
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);


            var response = await client.GetAsync("http://10.11.12.13:5010/identity/GetUserClaims");
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine("response.StatusCode");
                Console.WriteLine(response.StatusCode);
            }
            else
            {
                var content = response.Content.ReadAsStringAsync().Result;
                Console.WriteLine("API response.Content");
                //Console.WriteLine(JArray.Parse(content));
                ViewData["Content"] = content;
            }


            return View();
        }



        public async Task Logout()
        {
            await HttpContext.SignOutAsync("Cookies");
            await HttpContext.SignOutAsync(OpenIdConnectDefaults.AuthenticationScheme);
        }

        public IActionResult Error()
        {
            return View();
        }
    }
}