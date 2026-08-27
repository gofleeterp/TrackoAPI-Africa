using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global.CronJobs;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers.Global
{
    [AuthorizeEx]
    public class JobRetryLogsController : ODataController
    //ODataController
    {
        private readonly IService<JobRetryLog> _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public JobRetryLogsController(IUnitOfWorkAsync unitOfWorkAsync, IService<JobRetryLog> service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/JobRetryLog
        [HttpGet, EnableQuery]
        public IQueryable<JobRetryLog> Get() => _repo.Queryable();

        // GET: odata/JobRetryLogs(5)
        [EnableQuery]
        public SingleResult<JobRetryLog> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        
        // POST: odata/JobRetryLogs
        public async Task<IHttpActionResult> Post(JobRetryLog objJobRetryLog)
        {
            objJobRetryLog.ObjectState = ObjectState.Added;
            _repo.Insert(objJobRetryLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objJobRetryLog);
        }

        

        ////// PATCH: odata/JobRetryLogs(5)
        ///// PATCH performs a partial update. The client specifies just the properties to update.
        //[AcceptVerbs("PATCH", "MERGE")]
        //public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<JobRetryLog> patch)
        //{
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }

        //    JobRetryLog objJobRetryLog = await _repo.FindAsync(key);
        //    if (objJobRetryLog == null)
        //    {
        //        return NotFound();
        //    }
        //    objJobRetryLog.ObjectState = ObjectState.Modified;
        //    patch.Patch(objJobRetryLog);
        //    try
        //    {
        //        await _unitOfWorkAsync.SaveChangesAsync();
        //    }
        //    catch (DbUpdateConcurrencyException)
        //    {
        //        if (!JobExists(key))
        //        {
        //            return NotFound();
        //        }
        //        if (_repo.Query(x => x.JobName == objJobRetryLog.JobName && x.Id != objJobRetryLog.Id).Select().Any())
        //        {
        //            throw new BusinessException(ErrorCode.GLB104, $"Job with name \"{objJobRetryLog.JobName}\" Already Exists");
        //        }
        //        throw;
        //    }

        //    return Updated(objJobRetryLog);
        //}
        // DELETE: odata/JobRetryLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objJobRetryLog = await _repo.FindAsync(key);
            if (objJobRetryLog == null)
            {
                return NotFound();
            }
            objJobRetryLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(objJobRetryLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }
        private bool JobExists(long key)
        {
            return _repo.Query(e => e.Id == key).Select().Any();
        }
        //private bool ContactBookExists(string firstName) => _repo.Query(e => e.FirstName == firstName).Select().Any();
        //private bool ContactBookExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}