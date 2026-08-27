using Microsoft.AspNet.SignalR;

using Repository.Pattern.Core.UnitOfWork;

using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.FMS.Tyres;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TransactionSupportLogsController : ODataController
    //ODataController
    {
        private readonly ITransactionSupportLogService _service;
        private IUnitOfWorkAsync _uow;
        public TransactionSupportLogsController(ITransactionSupportLogService service, IUnitOfWorkAsync uow)
        {
            _service = service;
            _uow = uow;
        }
        // GET: odata/TransactionSupportLogs
        [HttpGet, EnableQuery]
        public IQueryable<TransactionSupportLog> Get()
        {
            return _service.Queryable();
        }

        // GET: odata/TransactionSupportLogs(5)
        [EnableQuery]
        public SingleResult<TransactionSupportLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }

        public async Task<IHttpActionResult> Put(long key, TransactionSupportLog objReportParam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != objReportParam.Id)
            {
                return BadRequest();
            }
            objReportParam.ObjectState = ObjectState.Modified;
            _service.Update(objReportParam);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objReportParam);
        }
        // POST: odata/TransactionSupportLogs
        public async Task<IHttpActionResult> Post(TransactionSupportLog objReportParam)
        {
            objReportParam.ObjectState = ObjectState.Added;
            _service.Insert(objReportParam);
            await Request.GetContext().SaveChangesAsync();
            return Created(objReportParam);
        }
        //// PATCH: odata/TransactionSupportLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TransactionSupportLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            TransactionSupportLog objReportParam = await _service.FindAsync(key);
            if (objReportParam == null)
            {
                return NotFound();
            }
            objReportParam.ObjectState = ObjectState.Modified;
            patch.Patch(objReportParam);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objReportParam);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objReportParam = await _service.FindAsync(key);
            if (objReportParam == null)
            {
                return NotFound();
            }
            objReportParam.ObjectState = ObjectState.Deleted;
            _service.Delete(objReportParam);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
                
            }
            base.Dispose(disposing);
        }
    }
}