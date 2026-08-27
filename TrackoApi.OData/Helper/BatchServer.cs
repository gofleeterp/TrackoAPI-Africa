using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

namespace TrackoApi.OData.Helper
{
    public class BatchServer : HttpServer
    {
        private readonly HttpConfiguration _config;

        public BatchServer(HttpConfiguration configuration)
            : base(configuration)
        {
            _config = configuration;
        }

        protected override void Initialize()
        {
            var firstInPipeline = _config.MessageHandlers.FirstOrDefault();
            if (firstInPipeline != null && firstInPipeline.InnerHandler != null)
            {
                InnerHandler = firstInPipeline;
            }
            else
            {
                base.Initialize();
            }
        }
    }
}
