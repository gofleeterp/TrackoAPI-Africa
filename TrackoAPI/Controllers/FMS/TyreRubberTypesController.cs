using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TyreRubberTypesController : ODataController
    //ODataController
    {
        private readonly ITyreRubberTypeService _objTyreRubberTypeService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public TyreRubberTypesController(IUnitOfWorkAsync unitOfWorkAsync, ITyreRubberTypeService service)
        {
            _objTyreRubberTypeService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/TyreRubberTypes
        [HttpGet, EnableQuery]
        public IQueryable<TyreRubberType> Get()
        {
            return _objTyreRubberTypeService.Queryable();
        }
        // GET: odata/TyreRubberTypes(5)
        [EnableQuery]
        public SingleResult<TyreRubberType> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objTyreRubberTypeService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/TyreRubberTypes(5)
        public async Task<IHttpActionResult> Put(long key, TyreRubberType objTyreRubberType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTyreRubberType.Id)
            {
                return BadRequest();
            }
            objTyreRubberType.ObjectState = ObjectState.Modified;
            _objTyreRubberTypeService.Update(objTyreRubberType);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TyreRubberTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTyreRubberType);
        }
        // POST: odata/TyreRubberTypes
        public async Task<IHttpActionResult> Post(TyreRubberType objTyreRubberType)
        {
            objTyreRubberType.ObjectState = ObjectState.Added;
            _objTyreRubberTypeService.Insert(objTyreRubberType);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TyreRubberTypeExists(objTyreRubberType.RubberType))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objTyreRubberType);
        }
        //// PATCH: odata/TyreRubberTypes(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TyreRubberType> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TyreRubberType objTyreRubberType = await _objTyreRubberTypeService.FindAsync(key);
            if (objTyreRubberType == null)
            {
                return NotFound();
            }
            objTyreRubberType.ObjectState = ObjectState.Modified;
            patch.Patch(objTyreRubberType);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TyreRubberTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objTyreRubberType);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTyreRubberType = await _objTyreRubberTypeService.FindAsync(key);
            if (objTyreRubberType == null)
            {
                return NotFound();
            }
            objTyreRubberType.ObjectState = ObjectState.Deleted;
            _objTyreRubberTypeService.Delete(objTyreRubberType);
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

        private bool TyreRubberTypeExists(string rubberType)
        {
            return _objTyreRubberTypeService.Query(e => e.RubberType == rubberType).Select().Any();
        }
        private bool TyreRubberTypeExists(long key)
        {
            return _objTyreRubberTypeService.Query(e => e.Id == key).Select().Any();
        }
    }
}