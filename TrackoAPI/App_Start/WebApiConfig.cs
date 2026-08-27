using AutoMapper;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OfficeOpenXml.Style;
using System.Drawing;
using System.Linq;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Web.Http;
using System.Web.Http.Cors;
using System.Web.Http.Dispatcher;
using System.Web.Http.ExceptionHandling;
using System.Web.OData.Batch;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;
using System.Web.OData.Formatter;
using System.Web.OData.Formatter.Deserialization;

using Tenant.OData;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.BMS;
using TrackoApi.OData;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.WebUtilities.Filters;
using TrackoAPI.WebUtilities.Formatters;
using TrackoAPI.WebUtilities.Formatters.WebApiContrib.Formatting.Xlsx;
using TrackoAPI.WebUtilities.Handler;
using TrackoAPI.WebUtilities.Helper;

using Unity;

namespace TrackoAPI
{

    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config,IUnityContainer container)
        {
            
            var corsAttr = new EnableCorsAttribute("*", "*", "*");
            config.EnableCors(corsAttr);
            WebApiUnityActionFilterProvider.RegisterFilterProviders(config);
            config.MessageHandlers.Add(new MethodOverrideHandler());
            //var builder = TelemetryConfiguration.Active.TelemetryProcessorChainBuilder;
            //builder.Use((next) => new FilterSQLTelemetryProcessor(next)).Use((next)=>new FilterExceptionlessTelemetryProcessor(next));
            // builder.Build();
            //Added to Log Exception
            //config.Services.Add(typeof(IExceptionLogger),new AiExceptionLogger());
            //config.Filters.Add(new CustomResponseHeaderFilter());
            config.MessageHandlers.Add(new EncodingDelegateHandler());
            config.MessageHandlers.Add(new DirectImageAccessHandler());
            if (!Helper.HostedOnPremise)
            {
                config.MessageHandlers.Add(new WebApiUsageHandler());
            }
            config.Services.Replace(typeof(IExceptionHandler),new ExceptionHandlerExt());
            //config.Services.Replace(typeof(IHostBufferPolicySelector), new NoBufferPolicySelector());
            config.Filters.Add(new ValidationActionFilter());

            // Web API routes
            //var model = Configure.GetEdmModel();
            var builder = Configure.GetEdmModelBuilder();
            var model=builder.GetEdmModel();

            var tenant_builder = TenantConfigure.GetEdmModelBuilder();
            var tenant_model = tenant_builder.GetEdmModel();

            config.MapHttpAttributeRoutes();
            config.EnableCaseInsensitive(true);
            config.EnableAlternateKeys(true);
            config.IncludeErrorDetailPolicy=IncludeErrorDetailPolicy.Always;
            var server = new BatchServer(config);
            ODataBatchHandler odataBatchHandler = new ODataBatchHandlerSingleTransaction(server);

            //config.MapODataServiceRoute("TenantODataRoute", "tenant", tenant_model, new ODataBatchHandlerSingleTransactionForTenant(server));  
            config.MapODataServiceRoute("ODataRoute", "odata", model, odataBatchHandler);
            //config.Services.Replace(typeof(IHttpControllerSelector), new GenericHttpControllerSelector(config,"tenant", tenant_builder.EntitySets, container));
            

            //help:https://github.com/OData/WebApi/blob/7584dffd1daa7ac64487c43160c4c2037fce905d/_posts/2015-01-16-06-01-custom-url-parsing.md
            //User can configure as below to support basic unqualified function/action call.
            config.EnableUnqualifiedNameCall(true);
            //User can configure as below to support basic string as enum parser behavior.
            config.EnableEnumPrefixFree(enumPrefixFree: true);
            // Web API configuration and services
            config.Routes.MapHttpRoute(
                name: "DefaultApi",
                routeTemplate: "api/{controller}/{id}",
                defaults: new { id = RouteParameter.Optional });
            //config.Routes.MapHttpRoute(
            //    name: "ClientSettings",
            //    routeTemplate: "api/{controller}/{id}",
            //    defaults: new { id = RouteParameter.Optional });
            // To disable tracing in your application,
            // please comment out or remove the following line of code
            // For more information, refer to: http://www.asp.net/web-api
            config.IncludeErrorDetailPolicy = IncludeErrorDetailPolicy.LocalOnly;
            var odataFormatters = ODataMediaTypeFormatters.Create(new NullSerializerProvider(), new DefaultODataDeserializerProvider());
            config.Formatters.InsertRange(0, odataFormatters);
            var jsonFormatter = config.Formatters.OfType<JsonMediaTypeFormatter>().First();
            jsonFormatter.SerializerSettings.ReferenceLoopHandling=ReferenceLoopHandling.Ignore;
            jsonFormatter.SerializerSettings.ContractResolver=new DeltaContractResolver();//new CamelCasePropertyNamesContractResolver();
            var xmlformater = GlobalConfiguration.Configuration.Formatters.XmlFormatter;
            xmlformater.UseXmlSerializer = true;
            config.Formatters.Add(xmlformater);
            var formatter = new XlsxMediaTypeFormatter(
                autoFilter: true,
                freezeHeader: true,
                headerHeight: 25f,
                cellStyle: (ExcelStyle s) =>
                {
                    s.Font.SetFromFont(new Font("Segoe UI", 13f, FontStyle.Regular));
                },
                headerStyle: (ExcelStyle s) =>
                {
                    s.Fill.PatternType = ExcelFillStyle.Solid;
                    s.Fill.BackgroundColor.SetColor(Color.FromArgb(0, 114, 51));
                    s.Font.Color.SetColor(Color.White);
                    s.Font.Size = 15f;                    
                });
            config.Formatters.Add(formatter);
            var textformater = new TextMediaTypeFormatter();
            textformater.AddQueryStringMapping("$format", "text-plain", new MediaTypeHeaderValue("text/plain"));
            textformater.AddQueryStringMapping("$format", "plain-text", new MediaTypeHeaderValue("text/plain"));
            textformater.AddQueryStringMapping("$format", "textplain", new MediaTypeHeaderValue("text/plain"));
            textformater.AddQueryStringMapping("$format", "plaintext", new MediaTypeHeaderValue("text/plain"));
            config.Formatters.Add(textformater);
            config.Formatters.JsonFormatter.SerializerSettings.Converters.Add(new StringEnumConverter());

            var configMapper = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<CnChallan, CnChallan>();
                cfg.CreateMap<vwBillPaymentLog, CNBillPaymentLog>();
                cfg.CreateMap<Voucher, Voucher>();
                cfg.CreateMap<CNBillLog, CNBillLog>().ForMember(x => x.Id, opt => opt.Ignore());
            });

            IMapper mapper = configMapper.CreateMapper();
            //config.Filters.Add(new CustomExceptionFilterAttribute());
        }        
        public static void RegisterGenericControllers(ODataConventionModelBuilder builder)
        {

        }
    }
}
