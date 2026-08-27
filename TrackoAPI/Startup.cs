using EntityFramework.BulkInsert;
using EntityFramework.BulkInsert.Providers;
using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.SQLite;
using HibernatingRhinos.Profiler.Appender.EntityFramework;
using Microsoft.AspNet.SignalR;
using Microsoft.AspNet.SignalR.Json;
using Microsoft.Owin;
using Microsoft.Owin.Cors;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.OAuth;
using Newtonsoft.Json;
using Owin;
using Repository.Pattern.Ef6;
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Diagnostics;
using System.Linq;
using System.Web.Http;
using System.Web.Http.ExceptionHandling;
using System.Web.OData.Extensions;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Providers;
using TrackoAPI.WebUtilities;
using TrackoAPI.WebUtilities.Handler;
using Unity;
using Unity.AspNet.WebApi;
using Unity.Config;


[assembly: OwinStartup(typeof(TrackoAPI.Startup))]
//[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(ApiDbConfiguration), "Start")]

namespace TrackoAPI
{
    //#if DEBUG
    public class ProfiledEfSqlBulkInsertProviderWithMappedDataReader : EfSqlBulkInsertProviderWithMappedDataReader
    {
        //protected override string ConnectionString => Tenant.ConnectionHelper.GetConnection();
    }

    //#endif
    public class Startup
    {
        public Startup()
        {
            //HangfireBootstrapper.Instance.Start();

            if (Helper.HanfireStorage==HangfireStorageType.redis)
            {
                Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions()
                {
                    AbortOnConnectFail = false,
                    EndPoints = { $"{Helper.RedisNetworkAddress}:{Helper.RedisPort}"},
                    Password = Helper.RedisPassword,
                    ConnectRetry = 10,
                    DefaultDatabase=2/*africa*/
                    //DefaultDatabase = 8923514
                }); /*"redis-10401.c57.us-east-1-4.ec2.cloud.redislabs.com:10401");*/
            }

        }

        public static OAuthBearerAuthenticationOptions OAuthBearerOptions { get; private set; }
        //public RedisConfiguration RadisCacheConfig { get; }

        public ConnectionMultiplexer Redis { get; private set; }

        public static HttpConfiguration config;

        public void Configuration(IAppBuilder app)
        {
            config = new HttpConfiguration();
            app.UseCors(CorsOptions.AllowAll);
            //#if DEBUG
            if (Helper.EFTracingFlag)
            {
                ProviderFactory.Register<ProfiledEfSqlBulkInsertProviderWithMappedDataReader>("HibernatingRhinos.Profiler.Appender.ProfiledDataAccess.ProfiledConnection`1[[System.Data.SqlClient.SqlClientFactory, System.Data, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089]]");
                HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler
                    .Initialize(new EntityFrameworkAppenderConfiguration()
                    {
                        HostToSendProfilingInformationTo = "localhost",
                        Port = 22898,
                    });
            }            
            
            ////#else
            ////            HibernatingRhinos.Profiler.Appender.EntityFramework.EntityFrameworkProfiler.InitializeForProduction(9090, "FfIVXyUkze38r/b2ulve26LQ88NK5AYig+ecYzp3r88=");
            //#endif
            // For more information on how to configure your application, visit http://go.microsoft.com/fwlink?LinkID=316888


            // #if DEBUG
            //var tracing= config.EnableSystemDiagnosticsTracing();
            // tracing.IsVerbose = true;
            // tracing.MinimumLevel = TraceLevel.Debug;
            // #endif
            config.AddODataQueryFilter();
            SqlServerTypes.Utilities.LoadNativeAssemblies(System.Web.Hosting.HostingEnvironment.MapPath("~/bin"));

            var unitConfig = UnityCore.Container;
            new UnityConfig(unitConfig);
            
            //app.UseRequestLifetimeMiddleware(unitConfig);
            var resolver = new UnityHierarchicalDependencyResolver(unitConfig);
            config.DependencyResolver = resolver;
            GlobalHost.DependencyResolver = new SignalRUnityDependencyResolver(unitConfig);
            var serializer = JsonUtility.CreateDefaultSerializer();
            serializer.ReferenceLoopHandling = ReferenceLoopHandling.Serialize;
            serializer.PreserveReferencesHandling = PreserveReferencesHandling.Objects;

            GlobalHost.DependencyResolver.Register(typeof(JsonSerializer), () => serializer);
            config.Services.Add(typeof(IExceptionLogger), new GofExceptionLogger());
            //FluentValidationModelValidatorProvider.Configure(config, p => p.ValidatorFactory = new UnityValidationFactory(unitConfig));
            ConfigureOAuth(app, config);

            app.UseStageMarker(PipelineStage.MapHandler);
            WebApiConfig.Register(config, unitConfig);
            app.Map("/fwlink", context => context.Use<ShortURLOwinMiddleware>());
            app.Map("/pubsub", map =>
            {
                // Setup the CORS middleware to run before SignalR.
                // By default this will allow all origins. You can
                // configure the set of origins and/or http verbs by
                // providing a cors options with a different policy.
                map.UseCors(CorsOptions.AllowAll);
                var hubConfiguration = new HubConfiguration
                {
                    // You can enable JSONP by uncommenting line below.
                    // JSONP requests are insecure but some older browsers (and some
                    // versions of IE) require JSONP to work cross domain
                    // EnableJSONP = true
                    EnableDetailedErrors = true,
                    EnableJavaScriptProxies = true
                };
                // Run the SignalR pipeline. We're not using MapSignalR
                // since this branch already runs under the "/signalr"
                // path.
                map.RunSignalR(hubConfiguration);
            });
            app.UseWebApi(config);
            
            var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById(Helper.CountryTimeZone);//TimeZoneInfo.Local;
            config.SetTimeZoneInfo(timeZoneInfo);
          
            if (Helper.HanfireStorage==HangfireStorageType.mssql)
            {
                Hangfire.GlobalConfiguration.Configuration.UseSqlServerStorage("HangFire");
            }
            else if(Helper.HanfireStorage == HangfireStorageType.redis)
            {
//#if DEBUG
//                if (Helper.HanfireStorage == HangfireStorageType.sqlite)
//                {
//                    var options = new SQLiteStorageOptions();
//                    Hangfire.GlobalConfiguration.Configuration.UseSQLiteStorage("SQLiteHangfire", options);
//                }
//                else
//                {
//                    Hangfire.GlobalConfiguration.Configuration.UseSqlServerStorage("HangFire");
//                }
//#else
            Hangfire.GlobalConfiguration.Configuration.UseRedisStorage(Redis);
//#endif
            }
            else if(Helper.HanfireStorage==HangfireStorageType.sqlite)
            {
                var options = new SQLiteStorageOptions();
                Hangfire.GlobalConfiguration.Configuration.UseSQLiteStorage("SQLiteHangfire", options);
            }
            Hangfire.GlobalConfiguration.Configuration.UseConsole();
            app.UseHangfireDashboard("/hangfire", new DashboardOptions
            {
                Authorization = new[] { new MyAuthorizationFilter() }
            });

            //app.UseHangfireDashboard("/hangfire");
            Hangfire.GlobalConfiguration.Configuration.UseUnityActivator(unitConfig);
            Hangfire.GlobalConfiguration.Configuration.UseNLogLogProvider();

            var queuesArray = new List<string>
            {
                "fifo_event_automation", "fifo_post_transaction", "default", "business_queue", "fifo_event_processing",
                "event_processing", "fifo_event_stockmerge"
            };
            try
            {
                queuesArray.AddRange(Helper.HangfireQueues);
            }
            catch
            {
                //Ignore
            }
            try
            {
                var _unity = UnityCore.Container;
                var _gs = _unity.Resolve<IGlobalStore>();
                var tenantKeySuffixList = _gs.Tenants.Select(x => $"fifo_{x.Value.Id.Replace("-", "").ToLower()}").ToArray();
                queuesArray.AddRange(tenantKeySuffixList);
            }
            catch
            {
                //Ignore
            }
            //if (!Debugger.IsAttached)
            //{

            //}

            app.UseHangfireServer(new BackgroundJobServerOptions
            {
                Queues = queuesArray.Distinct().ToArray(),
                WorkerCount = 10
            });
            int.TryParse(ConfigurationManager.AppSettings.GetValues("GPSSyncIntervalInMinutes")?.GetValue(0)?.ToString() ?? "26", out int gpsinterval);
            RecurringJob.AddOrUpdate<IHangfireJobProcessor>(x => x.SyncGPSStatusLog(null), /*Cron.MinuteInterval(gpsinterval)*/$"*/{gpsinterval} * * * *", timeZoneInfo);

 

            
            RecurringJob.AddOrUpdate<IHangfireJobProcessor>(x => x.CleanOldJobLogs(), /*Cron.HourInterval(8)*/$"0 */8 * * *", timeZoneInfo);
            //RecurringJob.AddOrUpdate<IHangfireJobProcessor>(x => x.SyncICICIFastTag(0,null), Cron.Hourly, timeZoneInfo);
            //RecurringJob.AddOrUpdate<IHangfireJobProcessor>(x => x.SyncICICIFastTagDaily(24,null), Cron.Daily(05, 00), timeZoneInfo);
            
            RecurringJob.AddOrUpdate<IHangfireJobProcessor>("MidNightGPSSync",x => x.SyncGPSStatusLog(null), /*Cron.MinuteInterval(gpsinterval)*/$"59 23 * * *", timeZoneInfo);
            RecurringJob.AddOrUpdate<IHangfireJobProcessor>("DayStartGPSSync", x => x.SyncGPSStatusLog(null), /*Cron.MinuteInterval(gpsinterval)*/$"00 00 * * *", timeZoneInfo);
            RecurringJob.AddOrUpdate<IHangfireJobProcessor>(x => x.TopupEmailFreeBalance(), Cron.Monthly(1), timeZoneInfo);
            
            BackgroundJob.Schedule<IHangfireJobProcessor>(x => x.ReRunAllCustomSchedule(null),TimeSpan.FromMinutes(15));





            new TrackoAPI.Reporting.Controller.Resolver();
            DbConfiguration.SetConfiguration(new ApiDbConfiguration());
            //config.Initializer(config);
            config.EnsureInitialized();
        }

        public void ConfigureOAuth(IAppBuilder app, HttpConfiguration config)
        {
            // Web API configuration and services
            // Configure Web API to use only bearer token authentication.
            // Configure the db context and user manager to use a single instance per request
            if (!Helper.HostedOnPremise)
            {
                app.CreatePerOwinContext(TenantDbContext.Create);
            }

            //app.Properties["httpConfig"] = config;
            //InteractiveViews.SetViewCacheFactory(this, new FileViewCacheFactory(AppDomain.CurrentDomain.BaseDirectory + "\\TrackoApiDbContext.views.xml"));
            OAuthBearerOptions = new OAuthBearerAuthenticationOptions
            {
                Challenge = "Bearer",
                AuthenticationType = "Bearer"
                // AccessTokenProvider = new OAuthBearerAuthenticationProviderEx("access_token")
            };
            var _unity = UnityCore.Container;
            var oAuthOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                AccessTokenExpireTimeSpan = TimeSpan.FromHours(8),
                TokenEndpointPath = new PathString("/token"),
                Provider = new SimpleOAuthProvider(_unity),
                RefreshTokenProvider = new SimpleRefreshTokenProvider(_unity)
                
            };
            // Token Generation
            app.UseOAuthAuthorizationServer(oAuthOptions);
            app.UseOAuthBearerAuthentication(OAuthBearerOptions);
            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);

        }
    }
    public class MyAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context)
        {
            var httpContext = context.GetOwinEnvironment();
            
            return true;
        }
    }

}