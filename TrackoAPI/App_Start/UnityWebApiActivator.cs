using Microsoft.Web.Infrastructure.DynamicModuleHelper;
using Repository.Pattern.Ef6;
using System.Web.Http;
using Unity.AspNet.WebApi;
using Unity.Config;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(TrackoAPI.UnityWebApiActivator), nameof(TrackoAPI.UnityWebApiActivator.Start))]
[assembly: WebActivatorEx.ApplicationShutdownMethod(typeof(TrackoAPI.UnityWebApiActivator), nameof(TrackoAPI.UnityWebApiActivator.Shutdown))]


namespace TrackoAPI
{
    /// <summary>
    /// Provides the bootstrapping for integrating Unity with WebApi when it is hosted in ASP.NET.
    /// </summary>
    public static class UnityWebApiActivator
    {
        public static ApiDbConfiguration ApiDbConfiguration { get; set; }
        /// <summary>
        /// Integrates Unity when the application starts.
        /// </summary>
        public static void Start() 
        {
            DynamicModuleUtility.RegisterModule(typeof(Unity.Config.UnityPerWebAPIRequestHttpModule));
            ApiDbConfiguration = new ApiDbConfiguration();
            // Use UnityHierarchicalDependencyResolver if you want to use
            // a new child container for each IHttpController resolution.
            // var resolver = new UnityHierarchicalDependencyResolver(UnityConfig.Container);           
            var resolver = new UnityDependencyResolver(UnityCore.Container);
            GlobalConfiguration.Configuration.DependencyResolver = resolver;
        }

        /// <summary>
        /// Disposes the Unity container when the application is shut down.
        /// </summary>
        public static void Shutdown()
        {
            UnityCore.Container.Dispose();
        }
    }
}