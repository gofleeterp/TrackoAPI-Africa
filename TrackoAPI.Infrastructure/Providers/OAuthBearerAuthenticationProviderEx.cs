using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Owin.Security.Infrastructure;
using Microsoft.Owin.Security.OAuth;

namespace TrackoAPI.Infrastructure.Providers
{
    public class OAuthBearerAuthenticationProviderEx: OAuthBearerAuthenticationProvider,IAuthenticationTokenProvider
    {
        private string _name;

        public OAuthBearerAuthenticationProviderEx()
        {
            
        }
        public OAuthBearerAuthenticationProviderEx(string name)
        {
            _name = name;
        }
        public override Task ValidateIdentity(OAuthValidateIdentityContext context)
        {
            var isValidated = context.IsValidated;
            return base.ValidateIdentity(context);
        }

        public override Task RequestToken(OAuthRequestTokenContext context)
        {
            if (string.IsNullOrWhiteSpace(_name)) return base.RequestToken(context);
            var value = context.Request.Query.Get(_name);

            if (string.IsNullOrEmpty(value)) return base.RequestToken(context);
            context.Token = value;
            return Task.FromResult<object>(null);
        }

        public void Create(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public Task CreateAsync(AuthenticationTokenCreateContext context)
        {
            throw new NotImplementedException();
        }

        public void Receive(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }

        public Task ReceiveAsync(AuthenticationTokenReceiveContext context)
        {
            throw new NotImplementedException();
        }
    }
}
