using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class SpareFitmentPositionsController : ODataController
    //ODataController
    {
        private readonly ISpareFitmentPositionService _objSpareFitmentPositionService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public SpareFitmentPositionsController(IUnitOfWorkAsync unitOfWorkAsync, ISpareFitmentPositionService service)
        {
            _objSpareFitmentPositionService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/SpareFitmentPositions
        [HttpGet, EnableQuery]
        public IQueryable<SpareFitmentPosition> Get()
        {
            return _objSpareFitmentPositionService.Queryable();
        }
        // GET: odata/SpareFitmentPositions(5)
        [EnableQuery]
        public SingleResult<SpareFitmentPosition> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objSpareFitmentPositionService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/SpareFitmentPositions(5)
        public async Task<IHttpActionResult> Put(long key, SpareFitmentPosition objSpareFitmentPosition)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objSpareFitmentPosition.Id)
            {
                return BadRequest();
            }
            objSpareFitmentPosition.ObjectState = ObjectState.Modified;
            _objSpareFitmentPositionService.Update(objSpareFitmentPosition);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objSpareFitmentPosition);
        }
        // POST: odata/SpareFitmentPositions
        public async Task<IHttpActionResult> Post(SpareFitmentPosition objSpareFitmentPosition)
        {
            objSpareFitmentPosition.ObjectState = ObjectState.Added;
            _objSpareFitmentPositionService.Insert(objSpareFitmentPosition);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objSpareFitmentPosition);
        }
        //// PATCH: odata/SpareFitmentPositions(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<SpareFitmentPosition> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SpareFitmentPosition objSpareFitmentPosition = await _objSpareFitmentPositionService.FindAsync(key);
            if (objSpareFitmentPosition == null)
            {
                return NotFound();
            }
            objSpareFitmentPosition.ObjectState = ObjectState.Modified;
            patch.Patch(objSpareFitmentPosition);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objSpareFitmentPosition);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objSpareFitmentPosition = await _objSpareFitmentPositionService.FindAsync(key);
            if (objSpareFitmentPosition == null)
            {
                return NotFound();
            }
            objSpareFitmentPosition.ObjectState = ObjectState.Deleted;
            _objSpareFitmentPositionService.Delete(objSpareFitmentPosition);
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

    }
}