using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using Microsoft.TeamFoundation.WorkItemTracking.WebApi.Models;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.TSVS;

namespace TrackoAPI.Controllers.Infrastructure
{
    [RoutePrefix("api/v2/support"),AuthorizeEx]
    public class TFSController : ApiController
    {
        private readonly TSVSClient tfsClient;
        public TFSController(TSVSClient tsvs_client)
        {
            tfsClient = tsvs_client;
        }
        [Route("CreateTicket/{projectName}/{workItemType}"), ResponseType(typeof(WorkItem)), HttpPost]
        public async Task<IHttpActionResult> CreateTicket([FromUri]string projectName, [FromUri]string workItemType, [FromBody]IDictionary<string, Object> fields)
        {
            var item = await tfsClient.CreateWorkItem(fields, workItemType, projectName);
            return Ok(item);
        }
        [Route("GetWorkItems/{projectName}"), ResponseType(typeof(List<WorkItem>)), HttpGet]
        public async Task<IHttpActionResult> GetWorkItems([FromUri]string projectName, [FromBody]WorkItemQuery query)
        {
            try
            {
                var item = await tfsClient.GetWorkItems(query.Query, query.Fields, projectName).ConfigureAwait(true);
                return Ok(item);
            }
            catch (Exception e)
            {
                return BadRequest(e.GetBaseException().Message);
            }
            
        }
    }
}
