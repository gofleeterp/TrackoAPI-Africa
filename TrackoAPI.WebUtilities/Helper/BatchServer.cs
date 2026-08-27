using System.Linq;
using System.Web.Http;

namespace TrackoAPI.WebUtilities.Helper
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
            if (firstInPipeline?.InnerHandler != null)
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
