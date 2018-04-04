using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Formatting;
using System.Web;
using System.Web.Http;
using log4net;
using Newtonsoft.Json;

namespace IdEpi.WebEpiserver.Business.Api
{
    public class WebApiConfig
    {
        #region Local variables

        private static readonly ILog Logger = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        #endregion

        #region Methods

        public static void Register(HttpConfiguration config)
        {

            config.Routes.MapHttpRoute(
                name: "WebApi",
                routeTemplate: "api/{controller}/{action}/{id}",
                defaults: new
                {
                    action = RouteParameter.Optional,
                    id = RouteParameter.Optional
                }
            );

            // Register validation filters
            //config.Filters.Add(new ValidationActionFilter());

            // json
            config.Formatters.JsonFormatter.MediaTypeMappings.Add(new RequestHeaderMapping(headerName: "Accept", headerValue: "text/html", valueComparison: StringComparison.InvariantCultureIgnoreCase, isValueSubstring: true, mediaType: "application/json"));

            // ignore loops
            config.Formatters.JsonFormatter.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        }

        #endregion

    }
}