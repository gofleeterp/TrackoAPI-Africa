using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global.CronJobs;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class MessagesAddressesController : ODataController
    //ODataController
    {
        private readonly IMessageAddressService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public MessagesAddressesController(IUnitOfWorkAsync unitOfWorkAsync, IMessageAddressService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/MessageAddress
        [HttpGet, EnableQuery]
        public IQueryable<MessageAddress> Get() => _repo.Queryable();

        // GET: odata/MessageAddresss(5)
        [EnableQuery]
        public SingleResult<MessageAddress> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/MessageAddresss(5)
        public async Task<IHttpActionResult> Put(long key, MessageAddress objMessageAddress)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objMessageAddress.Id)
            {
                return BadRequest();
            }
            objMessageAddress.ObjectState = ObjectState.Modified;
            _repo.Update(objMessageAddress);

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
                if (_repo.Query(x => x.ContactId == objMessageAddress.ContactId&&x.JobId== objMessageAddress.JobId && x.Id != objMessageAddress.Id).Select().Any())
                {
                    throw new BusinessException(ErrorCode.GLB104, "Contact Already Mapped with this job");
                }
                throw;
            }

            return Updated(objMessageAddress);
        }

        

        // POST: odata/MessageAddresss
        public async Task<IHttpActionResult> Post(MessageAddress objMessageAddress)
        {
            objMessageAddress.ObjectState = ObjectState.Added;
            _repo.Insert(objMessageAddress);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (JobExists(objMessageAddress))
                {
                    throw new BusinessException(ErrorCode.GLB104, $"Contact Already Mapped with this job");
                }
                throw;
            }
            return Created(objMessageAddress);
        }

        

        //// PATCH: odata/MessageAddresss(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<MessageAddress> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MessageAddress objMessageAddress = await _repo.FindAsync(key);
            if (objMessageAddress == null)
            {
                return NotFound();
            }
            objMessageAddress.ObjectState = ObjectState.Modified;
            patch.Patch(objMessageAddress);
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
                if (_repo.Query(x => x.ContactId == objMessageAddress.ContactId &&x.JobId==objMessageAddress.JobId&& (x.Id != objMessageAddress.Id)).Select().Any())
                {
                    throw new BusinessException(ErrorCode.GLB104, "Contact Already Mapped with this job");
                }
                throw;
            }

            return Updated(objMessageAddress);
        }
        // DELETE: odata/MessageAddresss(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objMessageAddress = await _repo.FindAsync(key);
            if (objMessageAddress == null)
            {
                return NotFound();
            }
            objMessageAddress.ObjectState = ObjectState.Deleted;
            _repo.Delete(objMessageAddress);
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
        private bool JobExists(MessageAddress MessageAddress)
        {
            return _repo.Query(e => e.ContactId == MessageAddress.ContactId&&e.JobId==MessageAddress.JobId).Select().Any();
        }
        //private bool ContactBookExists(string firstName) => _repo.Query(e => e.FirstName == firstName).Select().Any();
        //private bool ContactBookExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}