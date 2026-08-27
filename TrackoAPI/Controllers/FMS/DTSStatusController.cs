using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global.DTS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DTSStatusController : ODataController
    //ODataController
    {
        private readonly IDTSStatusService _service;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public DTSStatusController(IUnitOfWorkAsync unitOfWorkAsync, IDTSStatusService service)
        {
            _service = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/DTSStatuss
        [HttpGet, EnableQuery]
        public IQueryable<DTSStatus> Get()
        {
            return _service.Queryable();
        }
        // GET: odata/DTSStatuss(5)
        [EnableQuery]
        public SingleResult<DTSStatus> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/DTSStatuss(5)
        public async Task<IHttpActionResult> Put(long key, DTSStatus objDTSStatus)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDTSStatus.Id)
            {
                return BadRequest();
            }
            objDTSStatus.ObjectState = ObjectState.Modified;
            _service.Update(objDTSStatus);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DTSStatusExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDTSStatus);
        }
        // POST: odata/DTSStatuss
        public async Task<IHttpActionResult> Post(DTSStatus objDTSStatus)
        {
            objDTSStatus.ObjectState = ObjectState.Added;
            _service.Insert(objDTSStatus);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (DTSStatusExists(objDTSStatus.Name, objDTSStatus.Abbr))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name | Code already exists");
                }
                throw;
            }
            return Created(objDTSStatus);
        }
        //// PATCH: odata/DTSStatuss(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DTSStatus> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DTSStatus objDTSStatus = await _service.FindAsync(key);
            if (objDTSStatus == null)
            {
                return NotFound();
            }
            objDTSStatus.ObjectState = ObjectState.Modified;
            patch.Patch(objDTSStatus);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DTSStatusExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDTSStatus);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDTSStatus = await _service.FindAsync(key);
            if (objDTSStatus == null)
            {
                return NotFound();
            }
            objDTSStatus.ObjectState = ObjectState.Deleted;
            _service.Delete(objDTSStatus);
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

        private bool DTSStatusExists(string name,string code)
        {
            return _service.Query(e => e.Name == name || e.Abbr==code).Select().Any();
        }
        private bool DTSStatusExists(long key)
        {
            return _service.Query(e => e.Id == key).Select().Any();
        }

        [ODataRoute("DTSStatus({key})/StatusMappings")]
        public async Task<IHttpActionResult> PostStatusMappings([FromODataUri] long key, [FromBody] DTSStatusMapping status)
        {
            status.CurrentStatusId = key;
            var unitOfWorkAsync = Request.GetContext();
            unitOfWorkAsync.RepositoryAsync<DTSStatusMapping>().Insert(status);
            status.ObjectState=ObjectState.Added;
            await unitOfWorkAsync.SaveChangesAsync();
            return Created(status);
        }
    }
}