using Microsoft.Owin;
using Newtonsoft.Json;
using RedisCacheClient.UrlShortner;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tenant.Models;
using TrackoApi.Core.Helpers;
using Unity;
using Unity.Config;

namespace TrackoAPI.WebUtilities
{
    public class ShortURLOwinMiddleware : OwinMiddleware
    {

        public ShortURLOwinMiddleware(OwinMiddleware next) : base(next)
        {
            Next = next;
        }
        public override async Task Invoke(IOwinContext context)
        {
            try
            {


                var token = context.Request.Query["LinkID"] ?? context.Request.Query["linkid"];
                var Id = Base62Convertor.Decode(token.ToString());//IUrlRepository
                var urlRepository = UnityCore.Container.Resolve<IUrlRepository>();
                var urlData = await urlRepository.GetUrl(Id);
                if (string.IsNullOrWhiteSpace(urlData))
                {
                    await context.Response.WriteAsync("Invalid Url");
                    return;
                }
                var urlModel = JsonConvert.DeserializeObject<URLShortener>(urlData);
                if(urlModel==null||string.IsNullOrWhiteSpace(urlModel?.LongUrl))
                {
                    await context.Response.WriteAsync("Invalid Url");
                    return;
                }
                if (urlModel?.Headers != null && urlModel.Headers.Any())
                {
                    foreach (var header in urlModel.Headers)
                    {
                        context.Response.Headers.Add(header.Key, new[] { header.Value });
                    }
                }
                context.Response.Redirect(new Uri(urlModel.LongUrl).AbsoluteUri);                
                return;
            }
            catch (Exception ex)
            {
                await context.Response.WriteAsync("Something went wrong");
            }
            // Debugger.Break();
            //await context.Response.WriteAsync($"{token}:{Id}");
        }
        
    }
}
