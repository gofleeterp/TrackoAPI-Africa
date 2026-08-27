using Repository.Pattern.Core.Repositories;

using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;

using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.FMS
{
    public class GTranLogsController : ODataController
    {
        private readonly IRepositoryAsync<GeneralTransLog> _gtransRepo;

        public GTranLogsController(IRepositoryAsync<GeneralTransLog> service)
        {
            _gtransRepo = service;
        }
        // GET: odata/GTranLogs
        [HttpGet, EnableQuery]
        public IQueryable<GeneralTransLog> Get()
        {
            return _gtransRepo.Queryable();
        }
        // GET: odata/GTranLogs(5)
        [EnableQuery]
        public SingleResult<GeneralTransLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_gtransRepo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/GTranLogs(5)
        public async Task<IHttpActionResult> Put(long key, GeneralTransLog objGeneralTransLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objGeneralTransLog.Id)
            {
                return BadRequest();
            }
            objGeneralTransLog.ObjectState = ObjectState.Modified;
            _gtransRepo.Update(objGeneralTransLog);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objGeneralTransLog);
        }
        // POST: odata/GTranLogs
        public async Task<IHttpActionResult> Post(GeneralTransLog objGeneralTransLog)
        {
            objGeneralTransLog.ObjectState = ObjectState.Added;
            _gtransRepo.Insert(objGeneralTransLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(objGeneralTransLog);
        }
        //// PATCH: odata/GTranLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<GeneralTransLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            GeneralTransLog objGeneralTransLog = await _gtransRepo.FindAsync(key);
            if (objGeneralTransLog == null)
            {
                return NotFound();
            }
            objGeneralTransLog.ObjectState = ObjectState.Modified;
            patch.Patch(objGeneralTransLog);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objGeneralTransLog);
        }
        // DELETE: odata/GTranLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objGeneralTransLog = await _gtransRepo.FindAsync(key);
            if (objGeneralTransLog == null)
            {
                return NotFound();
            }
            objGeneralTransLog.ObjectState = ObjectState.Deleted;
            _gtransRepo.Delete(objGeneralTransLog);
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
