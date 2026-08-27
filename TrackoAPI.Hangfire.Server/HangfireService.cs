using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Hangfire.Console;
using Microsoft.Owin;
using Microsoft.Owin.Hosting;
using StackExchange.Redis;
using StackExchange.Redis.Extensions.Core.Configuration;
using TrackoApi.Core.Helpers;

namespace HangfireService
{
    public partial class HangfireService : ServiceBase
    {
        private IDisposable _apiserver = null;

        private BackgroundJobServer _hangfireServer;
#if !DEBUG
        public ConnectionMultiplexer Redis { get; private set; }
#endif

        public HangfireService()
        {
            InitializeComponent();
#if !DEBUG
            Redis = ConnectionMultiplexer.Connect(new ConfigurationOptions()
            {
                AbortOnConnectFail = false,
                EndPoints = { "43.240.65.34:6379" },
                Password = "YUib__(*)@#_($&lt;l__@#",
                ConnectRetry = 10
                //DefaultDatabase = 8923514
            }); /*"redis-10401.c57.us-east-1-4.ec2.cloud.redislabs.com:10401");*/
#endif
            if (Helper.HostedOnPremise)
            {
#if DEBUG
                Hangfire.GlobalConfiguration.Configuration.UseSqlServerStorage("HangFire");
#else
            Hangfire.GlobalConfiguration.Configuration.UseSqlServerStorage("HostedOnPremise");
#endif

            }
            else
            {
#if DEBUG
                Hangfire.GlobalConfiguration.Configuration.UseSqlServerStorage("HangFire");
#else
            Hangfire.GlobalConfiguration.Configuration.UseRedisStorage(Redis);
#endif
            }
        }

        protected override void OnStart(string[] args)
        {
            Trace.Listeners.Clear();
            Trace.Listeners.Add(new CustomTraceListener());//添加自定义监听器

            //Services URI
            string serveruri = System.Configuration.ConfigurationManager.AppSettings["WebAPIServerURI"].ToString();
            // Start OWIN host
            _apiserver = WebApp.Start<Startup>(url: serveruri);
            Hangfire.GlobalConfiguration.Configuration.UseConsole();
            
            _hangfireServer = new BackgroundJobServer();

            
            Trace.Write("Hangfire Service Start...");
        }

        protected override void OnStop()
        {
            if (_apiserver != null)
                _apiserver.Dispose();

            _hangfireServer.Dispose();

            Trace.Write("Hangfire Service Stop...");
        }
    }
}
