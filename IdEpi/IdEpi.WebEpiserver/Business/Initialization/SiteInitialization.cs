using System;
using System.Web.Http;
using System.Web.Mvc;
using EPiServer.Editor;
using EPiServer.Framework;
using EPiServer.Framework.Initialization;
using EPiServer.ServiceLocation;
using IdEpi.WebEpiserver.Business.Api;
using IdEpi.WebEpiserver.Business.DI;
using IdEpi.WebEpiserver.Business.DI.WebApi;
using IdEpi.WebEpiserver.Business.Facades;
using IdEpi.WebEpiserver.Business.Rendering;
using Newtonsoft.Json;

namespace IdEpi.WebEpiserver.Business.Initialization
{
    [ModuleDependency(typeof(ServiceContainerInitialization))]
    [InitializableModule]
    public class SiteInitialization : IConfigurableModule
    {
        public void ConfigureContainer(ServiceConfigurationContext context)
        {
            var services = context.Services;

            // add custom services

            //Register for auto injection of edit mode check, should be default life cycle (per request to service locator)
            services.AddTransient<IsInEditModeAccessor>(locator => () => PageEditing.PageIsInEditMode);

            // Configure the MVC StructureMapDependencyResolver
            DependencyResolver.SetResolver(new StructureMapDependencyResolver(context.StructureMap()));

            // WebApi StructureMapResolver
            GlobalConfiguration.Configure(config =>
            {
                config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
                config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings();
                config.Formatters.XmlFormatter.UseXmlSerializer = true;
                config.DependencyResolver = new StructureMapResolver(context.StructureMap());
                config.MapHttpAttributeRoutes();
            });


        }

        public void Initialize(InitializationEngine context)
        {
            // Register view engine
            ViewEngines.Engines.Insert(0, new SiteViewEngine());

            WebApiConfig.Register(GlobalConfiguration.Configuration);
            Console.WriteLine("WebApiConfig.Register");

            AreaRegistration.RegisterAllAreas();
        }

        public void Uninitialize(InitializationEngine context)
        {
        }
    }
}