using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IdEpi.Api.Controllers
{
    [Route("[controller]")]
    [Authorize]
    public class IdentityController : ControllerBase
    {
        const string authority = "http://10.11.12.13:5000";

        [HttpGet]
        //[Authorize(Roles = "WebEditors")]
        public async Task<IActionResult> Get()
        {
            Console.WriteLine("User Claims");
            User.Claims.ToList().ForEach(claim => Console.WriteLine("{0} {1}", claim.Type, claim.Value));
            Console.WriteLine(Environment.NewLine);

            var claimsValues = User.Claims.Select(claim => new
            {
                claim.Type,
                claim.Value
            });

            DiscoveryClient discoInstance = new DiscoveryClient(authority: authority)
            {
                Policy = new DiscoveryPolicy { RequireHttps = false } // For development
            };

            DiscoveryResponse disco = await discoInstance.GetAsync();

            var accessToken = await HttpContext.GetTokenAsync(tokenName: "access_token");


            var userInfo = new UserInfoClient(disco.UserInfoEndpoint);
            var userInfoResponse = await userInfo.GetAsync(accessToken);

            if (!userInfoResponse.IsError)
            {
                Console.WriteLine("Claims for the user");
                Console.WriteLine(userInfoResponse.Json.ToString());
                //userInfoResponse.Claims.ToList().ForEach(claim => Console.WriteLine("{0}: {1}", claim.Type, claim.Value));
                Console.WriteLine("\n\n");
            }
            else
            {
                Console.WriteLine("userInfoResponse.IsError");
            }


            // from c in User.Claims select new { c.Type, c.Value }
            //return new JsonResult(claimsValues);
            return new JsonResult(userInfoResponse.Json.ToString());
        }
    }
}
