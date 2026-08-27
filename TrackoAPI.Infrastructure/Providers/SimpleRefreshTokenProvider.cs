using Microsoft.Owin.Security.Infrastructure;
using System;
using System.Linq;
using System.Threading.Tasks;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Global;
using Unity;

namespace TrackoAPI.Infrastructure.Providers
{
    public class SimpleRefreshTokenProvider:IAuthenticationTokenProvider
    {
        private IUnityContainer _unity;
        private IGlobalStore globalstore;

        public SimpleRefreshTokenProvider(IUnityContainer unityContainer)
        {
            this._unity = unityContainer;
            globalstore = _unity.Resolve<IGlobalStore>();
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public async Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            var clientid = context.Ticket.Properties.Dictionary["client_id"];//applicationId
            if (string.IsNullOrWhiteSpace(clientid))
            {
                return;
            }
            var refreshTokenId = Guid.NewGuid().ToString("N");
            //using (var auth = new AuthRepository())
            //var config = context.OwinContext.Get<HttpConfiguration>("httpConfig");
            //var uhdr = (UnityHierarchicalDependencyResolver)config.DependencyResolver;
            //var _unity = ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer)));
            using (AuthRepository auth = (AuthRepository)_unity.Resolve<IAuthRepository>())
            {
                var refreshTokenLifeTime = context.OwinContext.Get<string>("as:clientRefreshTokenLifeTime");
                var claim = context.Ticket.Identity.Claims.FirstOrDefault(x => x.Type == "sub");
                if (claim == null)
                {
                    return;
                }
                var token=new ApiRefreshToken()
                {
                    Id=Helper.GetHash(refreshTokenId),
                    ClientKey = clientid,
                    Subject = claim.Value,
                    IssuedUtc = DateTime.Now,
                    ExpiresUtc = DateTime.Now.AddHours(Convert.ToDouble(refreshTokenLifeTime))
                };
                context.Ticket.Properties.IssuedUtc = token.IssuedUtc;
                context.Ticket.Properties.ExpiresUtc = token.ExpiresUtc;
                context.Ticket.Properties.AllowRefresh = true;                
                token.ProtectedTicket = context.SerializeTicket();
                var result = await auth.AddRefreshToken(token);
                if (result)
                {
                    var tenantid = Helper.LoggedInTenantId;
                    globalstore.AddToken(tenantid, token.Id,expiry:TimeSpan.FromHours(Convert.ToDouble(refreshTokenLifeTime)));
                    //GlobalStore.Instance.AccessTokens.AddOrUpdate(tenantid, new List<string>(), (s, list) =>
                    //{
                    //    list.Add(token.Id);
                    //    return list;
                    //});
                    //if (!GlobalStore.Instance.AccessTokens.ContainsKey(tenantid))
                    //{
                    //    GlobalStore.Instance.AccessTokens.TryAdd(tenantid, new List<string> { token.Id });
                    //}
                    //else
                    //{
                    //    GlobalStore.Instance.AccessTokens[Helper.LoggedInTenantId].Add(token.Id);
                    //}
                    
                    context.SetToken(refreshTokenId);
                }
            }
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }

        public async Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin");
            context.OwinContext.Response.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });
            var hashedTokenId = Helper.GetHash(context.Token);

            //using (var auth = new AuthRepository())
            //var config = context.OwinContext.Get<HttpConfiguration>("httpConfig");
            //var uhdr = (UnityHierarchicalDependencyResolver)config.DependencyResolver;
            //var _unity = ((IUnityContainer)uhdr.GetService(typeof(IUnityContainer)));
            using (AuthRepository auth = (AuthRepository)_unity.Resolve<IAuthRepository>())
            {
                var refreshToken = await auth.FindRefreshToken(hashedTokenId);

                if (refreshToken != null)
                {
                    //Get protectedTicket from refreshToken class
                    context.DeserializeTicket(refreshToken.ProtectedTicket);
                    await auth.RemoveRefreshToken(hashedTokenId);
                }
            }
        }
    }
}
