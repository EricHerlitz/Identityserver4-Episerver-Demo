using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

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
            client.SetBearerToken(accessToken);

            var response = await client.GetAsync("http://localhost:5010/identity");
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