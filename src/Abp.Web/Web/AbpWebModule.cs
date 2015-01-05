using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Http;
using System.Web.Http.Dispatcher;
using System.Web.Http.Controllers;
using System.Net.Http.Formatting;
using Abp.Modules;
using Abp.Web.Configuration;
using Abp.Web.Mvc.Controllers;
using Abp.WebApi.Controllers;
using Abp.WebApi.Controllers.Filters;
using Abp.WebApi.Controllers.Dynamic.Selectors;
using Abp.WebApi.Controllers.Dynamic.Formatters;
using Abp.WebApi.Controllers.Dynamic;
using Abp.Localization.Sources.Xml;
using Newtonsoft.Json.Serialization;

namespace Abp.Web
{
    /// <summary>
    /// This module is used to use ABP in ASP.NET web applications.
    /// </summary>
    public class AbpWebModule : AbpModule
    {
        /// <inheritdoc/>
        public override void PreInitialize()
        {
            if (HttpContext.Current != null)
            {
                XmlLocalizationSource.RootDirectoryOfApplication = HttpContext.Current.Server.MapPath("~");
            }

            IocManager.Register<IAbpWebModuleConfiguration, AbpWebModuleConfiguration>();
            IocManager.AddConventionalRegistrar(new ControllerConventionalRegistrar());
            IocManager.AddConventionalRegistrar(new ApiControllerConventionalRegistrar());

        }

        /// <inheritdoc/>
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(Assembly.GetExecutingAssembly());

            ControllerBuilder.Current.SetControllerFactory(new WindsorControllerFactory(IocManager.IocContainer.Kernel));
            GlobalFilters.Filters.Add(new AbpHandleErrorAttribute());

            InitializeAspNetServices();
            InitializeFilters();
            InitializeFormatters();
            InitializeRoutes();

            Configuration.Localization.Sources.Add(new XmlLocalizationSource("AbpWeb", "Localization\\AbpWeb"));
        }


        private static void InitializeAspNetServices()
        {
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerSelector), new AbpHttpControllerSelector(GlobalConfiguration.Configuration));
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpActionSelector), new AbpApiControllerActionSelector());
            GlobalConfiguration.Configuration.Services.Replace(typeof(IHttpControllerActivator), new AbpControllerActivator());
        }

        private static void InitializeFilters()
        {
            GlobalConfiguration.Configuration.Filters.Add(new AbpExceptionFilterAttribute());
        }

        private static void InitializeFormatters()
        {
            GlobalConfiguration.Configuration.Formatters.Clear();
            var formatter = new JsonMediaTypeFormatter();
            formatter.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            GlobalConfiguration.Configuration.Formatters.Add(formatter);
            GlobalConfiguration.Configuration.Formatters.Add(new PlainTextFormatter());
        }

        private static void InitializeRoutes()
        {
            DynamicApiRouteConfig.Register();
        }

    }
}
