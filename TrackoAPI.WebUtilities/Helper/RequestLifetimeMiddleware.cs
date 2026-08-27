using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Owin;
using Microsoft.Practices.Unity;
using Unity;
using Unity.Container.Registration;

namespace TrackoAPI.WebUtilities.Helper
{
    public class RequestLifetimeMiddleware: OwinMiddleware
    {
        private readonly IUnityContainer unityContainer;

        public RequestLifetimeMiddleware(OwinMiddleware next, IUnityContainer container) : base(next)
        {
            unityContainer = container;
        }

        public override async Task Invoke(IOwinContext context)
        {
            // Wait for request
            await base.Next.Invoke(context);

            // Identify child controlled types
            var registrations = unityContainer.Registrations.Where(x => x.LifetimeManager is Unity.Lifetime.TransientLifetimeManager);
            foreach (ContainerRegistration type in registrations)
            {
                // Cleanup PerRequestLifetimeManager's mess
                var instance = unityContainer.Resolve(type.RegisteredType) as IDisposable;
                instance?.Dispose();
            }
        }
    }
}
