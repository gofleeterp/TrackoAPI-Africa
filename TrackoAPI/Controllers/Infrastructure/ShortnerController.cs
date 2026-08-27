using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using RedisCacheClient;
using RedisCacheClient.UrlShortner;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using Tenant.Models;
using TrackoApi.Core.Helpers;

namespace TrackoAPI.Controllers.Infrastructure
{
    [RoutePrefix("api/shortner")]
    public class ShortnerController : ApiController
    {
        private readonly IUniqueIdGenerator uniqueIdGenerator;
        private readonly IUrlRepository urlRepository;

        public ShortnerController(IUniqueIdGenerator uniqueIdGenerator, IUrlRepository urlRepository)
        {
            this.uniqueIdGenerator = uniqueIdGenerator;
            this.urlRepository = urlRepository;
        }

        [HttpPost,Route("simple")]
        public async Task<IHttpActionResult> CreateSimpleShortUrl([FromBody] string url)
        {
            try
            {
                var model = new URLShortener(url);
                bool isUri = Uri.IsWellFormedUriString(url, UriKind.Absolute);
                if (!isUri || !model.CheckLongUrlExists())
                {
                    return BadRequest("Not a valid URL!!! Come on you can do better");
                }                
                model.LinkId = await this.uniqueIdGenerator.GetNext();
                var shorturl = $"{Helper.UrlShortnerBaseAddress}/fwlink?LinkID={Base62Convertor.Convert(model.LinkId)}";
                model.ShortUrl = shorturl;
                if (model.CheckLongUrlExists())
                {
                    return BadRequest("Long Url is Invalid");
                }
                var persistanceStatus = await this.urlRepository.SaveUrl(model.LinkId, JsonConvert.SerializeObject(model),TimeSpan.FromTicks(model.ExpiryTicks));
                if (persistanceStatus == false)
                {
                    throw new Exception("Sorry!! We have some temporary down time. We request to retry after sometime");
                }
                return Created(new Uri(shorturl), shorturl);
            }
            catch (Exception ex)
            {
                throw new Exception("Sorry!! We have some temporary down time. We request to retry after sometime");
            }
            
        }
        [HttpPost, Route("advance")]
        public async Task<IHttpActionResult> CreateAdvanceShortUrl([FromBody]URLShortener urlModel)
        {
            try
            {
                bool isUri = Uri.IsWellFormedUriString(urlModel.LongUrl, UriKind.Absolute);
                if (!isUri|| !urlModel.CheckLongUrlExists())
                {
                    return BadRequest("Not a valid URL!!! Come on you can do better");
                }
                urlModel.LinkId =  await this.uniqueIdGenerator.GetNext();
                var shorturl = $"{Helper.UrlShortnerBaseAddress}/fwlink?LinkID={Base62Convertor.Convert(urlModel.LinkId)}";
                urlModel.ShortUrl = shorturl;
                
                var persistanceStatus = await this.urlRepository.SaveUrl(urlModel.LinkId, JsonConvert.SerializeObject(urlModel), TimeSpan.FromTicks(urlModel.ExpiryTicks));
                if (persistanceStatus == false)
                {
                    throw new Exception("Sorry!! We have some temporary down time. We request to retry after sometime");
                }
                return Created(new Uri(shorturl), shorturl);
            }
            catch (Exception ex)
            {
                throw new Exception("Sorry!! We have some temporary down time. We request to retry after sometime");
            }

        }
    }
}
