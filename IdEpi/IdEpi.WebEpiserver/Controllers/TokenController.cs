using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;

namespace IdEpi.WebEpiserver.Features.Api
{
    public class TokenController : ApiController
    {
        [HttpGet]
        public HttpResponseMessage Get(int id)
        {
            return Request.CreateResponse(HttpStatusCode.OK, id);
        }


        [Authorize]
        [HttpGet]
        public HttpResponseMessage GetAll()
        {
            var a = User.Identity.IsAuthenticated;


            return Request.CreateResponse(HttpStatusCode.OK, a);
        }

        
    }
}
