using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using Hangfire;

namespace TrackoAPI.App_Start
{
    public class HangfireBootstrapper : IRegisteredObject
    {
        public static readonly HangfireBootstrapper Instance = new HangfireBootstrapper();
        private readonly object _lockObject = new object();
        private bool _started;
        private BackgroundJobServer _backgroundJobServer;
        private HangfireBootstrapper()
        {
        }
        public void Start()
        {
            lock (_lockObject)
            {
                if (_started) return;
                _started = true;

                HostingEnvironment.RegisterObject(this);

                GlobalConfiguration.Configuration
                    .UseSqlServerStorage("HangFire");
                //// Specify other options here

                _backgroundJobServer = new BackgroundJobServer(new BackgroundJobServerOptions { ServerName = $"{Environment.MachineName}",HeartbeatInterval=TimeSpan.FromSeconds(10),ServerCheckInterval=TimeSpan.FromSeconds(10)});
                
            }
        }
        public void Stop()
        {
            lock (_lockObject)
            {
                _backgroundJobServer?.Dispose();
                HostingEnvironment.UnregisterObject(this);
            }
        }
        void IRegisteredObject.Stop(bool immediate)
        {
            Stop();
        }
    }
}