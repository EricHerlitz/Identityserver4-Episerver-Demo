using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using EPiServer.Security;
using EPiServer.ServiceLocation;

namespace IdEpi.WebEpiserver.Business.Identity
{
    [ServiceConfiguration(typeof(ISynchronizingUserService))]
    public class OicSynchronizingUserService : ISynchronizingUserService
    {
        public Task SynchronizeAsync(ClaimsIdentity identity, IEnumerable<string> additionalClaimsToSync)
        {
            // Do claim transforms here
            return Task.FromResult(0);
        }
    }
}