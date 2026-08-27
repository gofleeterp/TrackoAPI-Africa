using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ReportProcsController:ODataController
    {
        private readonly IRepositoryAsync<ReportProcedure> _service;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public ReportProcsController(IUnitOfWorkAsync unitOfWorkAsync, IRepositoryAsync<ReportProcedure> service)
        {
            _service = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/ReportProcs
        [HttpGet,EnableQuery]
        public IQueryable<ReportProcedure> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/ReportProcs(5)
        [EnableQuery]
        public SingleResult<ReportProcedure> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/ReportProcs(5)
       public async Task<IHttpActionResult> Put(long key, ReportProcedure ReportProcedure)
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }
        // POST: odata/ReportProcs
        public async Task<IHttpActionResult> Post(ReportProcedure ReportProcedure)
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }
        //// PATCH: odata/ReportProcs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ReportProcedure> patch)
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }
        
    }
}