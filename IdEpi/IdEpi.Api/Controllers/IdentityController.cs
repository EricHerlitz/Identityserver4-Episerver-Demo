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
    //[Authorize()]
    public class IdentityController : ControllerBase
    {

        [HttpGet]
        [Route("GetUserClaims")]
        //[Authorize(Roles = "WebEditors")]
        public async Task<IActionResult> GetUserClaims()
        {
            Console.WriteLine("Returning registered user claims");

            var claimsValues = User.Claims.Select(claim => new
            {
                claim.Type,
                claim.Value
            });
            
            return new JsonResult(value: claimsValues);
        }

        [HttpGet]
        [Route("GetUserClaimsDisco")]
        [Authorize(Roles = "WebEditors")]
        public async Task<IActionResult> GetUserClaimsDisco()
        {
            Console.WriteLine("Fetching claims from IdentityServer using DiscoveryClient");

            DiscoveryClient discoInstance = new DiscoveryClient(authority: Startup.Authority)
            {
                Policy = new DiscoveryPolicy { RequireHttps = false } // For development
            };

            DiscoveryResponse disco = await discoInstance.GetAsync();

            var accessToken = await HttpContext.GetTokenAsync(tokenName: "access_token");
            var userInfo = new UserInfoClient(endpoint: disco.UserInfoEndpoint);
            var userInfoResponse = await userInfo.GetAsync(token: accessToken);

            if (userInfoResponse.IsError)
            {
                Console.WriteLine("userInfoResponse.IsError");
                throw userInfoResponse.Exception;
            }

            return new JsonResult(value: userInfoResponse.Json.ToString());
        }
    }
}
