using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using EPiServer;
using EPiServer.Core;
using EPiServer.Framework.DataAnnotations;
using EPiServer.Web.Mvc;
using IdentityModel.Client;
using Microsoft.Owin.Security;

namespace IdEpi.WebEpiserver.Features.Pages.Start
{
    public class StartPageController : PageController<StartPage>
    {
        private readonly IContentLoader _contentLoader;

        public StartPageController(IContentLoader contentLoader)
        {
            _contentLoader = contentLoader;
        }

        //[Authorize]
        public async Task<ActionResult> Index(StartPage currentPage)
        {
            List<Claim> claims = null;

            // If logged in
            if (User.Identity.IsAuthenticated)
            {
                var user = User as ClaimsPrincipal;

                // call api
                var client = new HttpClient();
                //client.SetBearerToken(accessToken); // if IdentityModel is installed
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", user.FindFirst("access_token").Value);
                var response = await client.GetAsync("http://10.11.12.13:5010/identity/GetUserClaims");

                if (!response.IsSuccessStatusCode)
                {
                    // On error
                    Console.WriteLine(response.StatusCode);
                }
                else
                {
                    var content = response.Content.ReadAsStringAsync().Result;
                    Console.WriteLine("API response.Content");
                    Console.WriteLine(content);
                    ViewData["Content"] = content;
                }


            }

            var viewModel = new StartPageViewModel(currentPage)
            {
                TestProp = _contentLoader.Get<PageData>(ContentReference.RootPage).Name,
                Claims = claims
            };

            return View(viewModel);
        }

        [Authorize]
        public ActionResult Secure(StartPage currentPage)
        {
            var a = User.Identity.IsAuthenticated;

            var viewModel = new StartPageViewModel(currentPage)
            {
                TestProp = a.ToString()
            };

            return View("~/Views/Pages/StartPage/StartPageIndex.cshtml", viewModel);
        }

    }
}