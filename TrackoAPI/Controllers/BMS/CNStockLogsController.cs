using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.BMS;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    
    [AuthorizeEx]
    public class CNStockLogsController : ODataController
    {
        private readonly ICNStockLogService _repo;
        public CNStockLogsController(ICNStockLogService service)
        {
            _repo = service;
        }
        // GET: odata/cNStockLogs
        [HttpGet, EnableQuery]
        public IQueryable<CNStockLog> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/cNStockLogs(5)
        [EnableQuery]
        public SingleResult<CNStockLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        [HttpGet,EnableQuery]
        public IQueryable<vwCNStockSearch> SearchTop10CNStock([FromODataUri] long challanOfficeId,
            [FromODataUri] long stockOfficeId, [FromODataUri] DateTime challanDate, [FromODataUri] string serachTerm)
        {
            return _repo.GetTop10CnStock(challanOfficeId,stockOfficeId,challanDate,serachTerm);
        }
        
        // PUT: odata/cNStockLogs(5)
        public async Task<IHttpActionResult> Put(long key, CNStockLog cNStockLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != cNStockLog.Id)
            {
                return BadRequest();
            }
            cNStockLog.ObjectState = ObjectState.Modified;
            _repo.Update(cNStockLog);
            await Request.GetContext().SaveChangesAsync();

            return Updated(cNStockLog);
        }
        // POST: odata/cNStockLogs
        public async Task<IHttpActionResult> Post(CNStockLog cNStockLog)
        {
            cNStockLog.ObjectState = ObjectState.Added;

            var ch = _repo.Insert(cNStockLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(ch);
        }
        //// PATCH: odata/cNStockLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNStockLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CNStockLog ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            ch.ObjectState = ObjectState.Modified;
            patch.Patch(ch);
            await Request.GetContext().SaveChangesAsync();
            return Updated(ch);
        }
        // DELETE: odata/cNStockLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var cNStockLog = await _repo.FindAsync(key);
            if (cNStockLog == null)
            {
                return NotFound();
            }
            cNStockLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(cNStockLog);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
    }
    [AuthorizeEx]
    public class CNStockLogViewController : ODataController
    {
        private readonly IRepositoryAsync<vw_CNStockLog> _repo;

        public CNStockLogViewController(IRepositoryAsync<vw_CNStockLog> service)
        {
            _repo = service;
        }
        [HttpGet, EnableQuery]
        public IQueryable<vw_CNStockLog> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/cNStockLogs(5)
        [EnableQuery]
        public SingleResult<vw_CNStockLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
    }
    [AuthorizeEx]
    public class CNStockMMLogViewController : ODataController
    {
        private readonly IRepositoryAsync<vw_CNStockMMLog> _repo;

        public CNStockMMLogViewController(IRepositoryAsync<vw_CNStockMMLog> service)
        {
            _repo = service;
        }
        [HttpGet, EnableQuery]
        public IQueryable<vw_CNStockMMLog> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/cNStockLogs(5)
        [EnableQuery]
        public SingleResult<vw_CNStockMMLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
    }
}