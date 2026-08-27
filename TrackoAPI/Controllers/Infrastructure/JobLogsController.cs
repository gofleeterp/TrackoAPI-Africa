using Repository.Pattern.Core.UnitOfWork;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global.CronJobs;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers.Global
{
    [AuthorizeEx]
    public class JobLogsController : ODataController
    //ODataController
    {
        private readonly IJobLogService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public JobLogsController(IUnitOfWorkAsync unitOfWorkAsync, IJobLogService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/JobLog
        [HttpGet, EnableQuery]
        public IQueryable<JobLog> Get() => _repo.Queryable();

        // GET: odata/JobLogs(5)
        [EnableQuery]
        public SingleResult<JobLog> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/JobLogs(5)
        public async Task<IHttpActionResult> Put(long key, JobLog objJobLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objJobLog.Id)
            {
                return BadRequest();
            }
            objJobLog.ObjectState = ObjectState.Modified;
            _repo.Update(objJobLog);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobExists(key))
                {
                    return NotFound();
                }
                if (_repo.Query(x => x.JobName == objJobLog.JobName && x.Id != objJobLog.Id).Select().Any())
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Job with name \"{objJobLog.JobName}\" Already Exists");
                }
                throw;
            }

            return Updated(objJobLog);
        }

        

        // POST: odata/JobLogs
        public async Task<IHttpActionResult> Post(JobLog objJobLog)
        {
            objJobLog.ObjectState = ObjectState.Added;
            _repo.Insert(objJobLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (JobExists(objJobLog))
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Job with name \"{objJobLog.JobName}\" Already Exists");
                }
                throw;
            }
            return Created(objJobLog);
        }

        

        //// PATCH: odata/JobLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<JobLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            JobLog objJobLog = await _repo.FindAsync(key);
            if (objJobLog == null)
            {
                return NotFound();
            }
            objJobLog.ObjectState = ObjectState.Modified;
            patch.Patch(objJobLog);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!JobExists(key))
                {
                    return NotFound();
                }
                if (_repo.Query(x => x.JobName == objJobLog.JobName && x.Id != objJobLog.Id).Select().Any())
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Job with name \"{objJobLog.JobName}\" Already Exists");
                }
                throw;
            }

            return Updated(objJobLog);
        }
        // DELETE: odata/JobLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objJobLog = await _repo.FindAsync(key);
            if (objJobLog == null)
            {
                return NotFound();
            }
            objJobLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(objJobLog);
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
        private bool JobExists(JobLog jobLog)
        {
            return _repo.Query(e => e.JobName == jobLog.JobName).Select().Any();
        }
        //private bool ContactBookExists(string firstName) => _repo.Query(e => e.FirstName == firstName).Select().Any();
        //private bool ContactBookExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
        [ODataRoute("JobLogs({key})/MessageAddresses")]
        public async Task<IHttpActionResult> PostMessageAddresses([FromODataUri] long key, [FromBody]MessageAddress address)
        {
            address.JobId = key;            
            var unitOfWorkAsync = Request.GetContext();
            var repo = unitOfWorkAsync.RepositoryAsync<MessageAddress>();
            var exisitng = await repo.Queryable().Where(x => x.ContactId == address.ContactId && x.JobId == key).Select(x=>x.fk_Contact.ContactValue).FirstOrDefaultAsync();
            if (!string.IsNullOrWhiteSpace(exisitng))
            {
                return BadRequest($"{exisitng} contact has been already added in against this Job.");
            }
            address.ObjectState = ObjectState.Added;
            repo.Insert(address);
            await  unitOfWorkAsync.SaveChangesAsync();
            return Created(address);
        }
        //[ODataRoute("JobLogs/{timeSpan}")]
        //public void ScheduleNewJob([FromBody] string action,[FromODataUri] double timeSpan = 0)
        //{
        //    CSharpScript.EvaluateAsync<LambdaExpression>(action);
        //    //var job=new BackgroundJob("",new Hangfire.Common.Job())
        //    //Hangfire.BackgroundJob.Schedule(Hangfire.JobAct, TimeSpan.FromSeconds(timeSpan));
        //}
    }
}