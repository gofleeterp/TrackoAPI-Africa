using IWLT.TrackoAPI.Subscription.Helpers;
using IWLT.TrackoAPI.Subscription.Middleware;
using IWLT.TrackoAPI.Subscription.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SpaServices.AngularCli;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.OData.Formatter.Deserialization;
using Microsoft.AspNetCore.OData.Routing;
using Microsoft.AspNetCore.OData.Routing.Template;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.OData.Extensions;
using Microsoft.OData.Edm;
using Newtonsoft.Json.Serialization;

using Polly;

using System;
using System.Net.Http;
using Microsoft.AspNetCore.OData.Batch;

namespace IWLT.TrackoAPI.Subscription
{
    public class Startup: DisposableObject
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var retryPolicy = Policy.TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(2));
            services.AddOptions();            
            services.AddCors(cors=>
            {
                var policy = new CorsPolicy();
                policy.IsOriginAllowed = (origin) => true;
                cors.AddDefaultPolicy(policy);
            });
            services.AddHttpClient();
            services.AddResponseCaching();
            services.AddHttpContextAccessor();
            services.AddODataQueryFilter();
            services.AddMvc().AddControllersAsServices().AddOData(op =>
            {
                op.EnableAttributeRouting = true;
                var timeZoneInfo = TimeZoneInfo.FindSystemTimeZoneById("India Standard Time");//TimeZoneInfo.Local;
                op.TimeZone = timeZoneInfo;
                op.AddRouteComponents("odata", ODataHelpers.GetEdmModel(),OnCreateODataBatchHandler()).EnableQueryFeatures();
            }).AddNewtonsoftJson(options=>
            {
                options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
            }).SetCompatibilityVersion(CompatibilityVersion.Latest);
            //services.AddMvc(options =>
            //{
            //    options.EnableEndpointRouting = false;
            //}).SetCompatibilityVersion(CompatibilityVersion.Version_3_0);
            
            // In production, the Angular files will be served from this directory
            services.AddSpaStaticFiles(configuration =>
            {
                configuration.RootPath = "ClientApp/dist";
            });
            services.AddDbContextPool<TenantDbContext>(options =>
                {
                    options.UseSqlServer(Configuration.GetConnectionString("TenantsContext"));
                });
        }
        public bool EnableODataBatchHandler { get; set; } = false;
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHostApplicationLifetime applicationLifetime)
        {
            UpdateDatabase(app);
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();                
            }
            
            app.UseHttpsRedirection();
            //app.UseODataBatching();
            app.UseMiddleware<ErrorLoggerMiddleware>();

            app.UseStaticFiles();
            if (!env.IsDevelopment())
            {
                app.UseSpaStaticFiles();
            }
            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseMvc(routes =>
            //{
            //    routes.MapODataServiceRoute("odata", "odata", ODataHelpers.GetEdmModel(app.ApplicationServices));
            //    routes.GetDefaultODataOptions();
            //    routes.MapRoute(
            //        name: "default",
            //        template: "{controller}/{action=Index}/{id?}");
            //});
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultControllerRoute();                
                endpoints.MapControllers();
            });
            app.UseSpa(spa =>
            {
                // To learn more about options for serving an Angular SPA from ASP.NET Core,
                // see https://go.microsoft.com/fwlink/?linkid=864501

                spa.Options.SourcePath = "ClientApp";
                if (env.IsDevelopment())
                {
                    spa.Options.StartupTimeout = TimeSpan.FromSeconds(120);
                    spa.UseAngularCliServer(npmScript: "start");
                }
                
            });
            applicationLifetime.ApplicationStopping.Register(OnShutdown);
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<TenantDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
        private void OnShutdown()
        {
            Dispose();
        }
        protected override void OnDispose()
        {
            base.OnDispose();
            //have something to dispose? Write it here.
        }

        private ODataBatchHandler OnCreateODataBatchHandler()
        {
            ODataBatchHandler odataBatchHandler = new TransactionScopeODataBatchHandler();

            odataBatchHandler.MessageQuotas.MaxOperationsPerChangeset = 20;
            odataBatchHandler.MessageQuotas.MaxPartsPerBatch = 10;

            return odataBatchHandler;
        }
    }
}
