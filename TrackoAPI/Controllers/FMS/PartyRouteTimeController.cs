using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoAPI.Infrastructure.Filters;
using TrackoApi.Models.FMS;
using TrackoApi.Service.FMS;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class PartyRouteTimeController : ODataController
    //ODataController
    {
        private readonly IPartyRouteTimeService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public PartyRouteTimeController(IUnitOfWorkAsync unitOfWorkAsync, IPartyRouteTimeService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/PartyRouteTime
        [HttpGet, EnableQuery]
        public IQueryable<PartyRouteTime> Get() => _repo.Queryable();

        // GET: odata/PartyRouteTime(5)
        [EnableQuery]
        public SingleResult<PartyRouteTime> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/PartyRouteTime(5)
        public async Task<IHttpActionResult> Put(long key, PartyRouteTime objPartyRouteTime)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objPartyRouteTime.Id)
            {
                return BadRequest();
            }
            objPartyRouteTime.ObjectState = ObjectState.Modified;
            _repo.Update(objPartyRouteTime);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PartyRouteTimeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objPartyRouteTime);
        }

        // POST: odata/PartyRouteTime
        public async Task<IHttpActionResult> Post(PartyRouteTime objPartyRouteTime)
        {
            objPartyRouteTime.ObjectState = ObjectState.Added;
            _repo.Insert(objPartyRouteTime);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PartyRouteTimeExists(objPartyRouteTime.PartyId, objPartyRouteTime.RouteId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Party & Route are unique");
                }
                throw;
            }
            return Created(objPartyRouteTime);
        }
        //// PATCH: odata/PartyRouteTime(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PartyRouteTime> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PartyRouteTime objPartyRouteTime = await _repo.FindAsync(key);
            if (objPartyRouteTime == null)
            {
                return NotFound();
            }
            objPartyRouteTime.ObjectState = ObjectState.Modified;
            patch.Patch(objPartyRouteTime);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PartyRouteTimeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objPartyRouteTime);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objPartyRouteTime = await _repo.FindAsync(key);
            if (objPartyRouteTime == null)
            {
                return NotFound();
            }
            objPartyRouteTime.ObjectState = ObjectState.Deleted;
            _repo.Delete(objPartyRouteTime);
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

        private bool PartyRouteTimeExists(long partyId,long routeId) => _repo.Query(e => e.PartyId == partyId|| e.RouteId== routeId).Select().Any();
        private bool PartyRouteTimeExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}