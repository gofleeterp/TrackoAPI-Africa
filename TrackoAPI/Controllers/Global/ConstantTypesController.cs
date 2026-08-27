using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ConstantTypesController:ODataController
    {
        private readonly IConstantTypeService _constantTypeService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public ConstantTypesController(IUnitOfWorkAsync unitOfWorkAsync, IConstantTypeService service)
        {
            _constantTypeService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/ConstantTypes
        [HttpGet,EnableQuery]
        public IQueryable<ConstantType> GetConstantTypes()
        {
            return _constantTypeService.Queryable();
        }
        // GET: odata/ConstantTypes(5)
        [EnableQuery]
        public SingleResult<ConstantType> GetConstantType([FromODataUri] long key)
        {
            return SingleResult.Create(_constantTypeService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/ConstantTypes(5)
       public async Task<IHttpActionResult> Put(long key, ConstantType constantType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != constantType.Id)
            {
                return BadRequest();
            }
            constantType.ObjectState=ObjectState.Modified;
            _constantTypeService.Update(constantType);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConstantTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(constantType);
        }
        // POST: odata/ConstantTypes
        public async Task<IHttpActionResult> Post(ConstantType constantType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            constantType.ObjectState = ObjectState.Added;
            _constantTypeService.Insert(constantType);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ConstantTypeExists(constantType.ConstantTypeName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }

            return Created(constantType);
        }
        //// PATCH: odata/ConstantTypes(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ConstantType> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ConstantType constantType = await _constantTypeService.FindAsync(key);

            if (constantType == null)
            {
                return NotFound();
            }
            constantType.ObjectState=ObjectState.Modified;
            patch.Patch(constantType);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConstantTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(constantType);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            ConstantType constantType = await _constantTypeService.FindAsync(key);

            if (constantType == null)
            {
                return NotFound();
            }
            constantType.ObjectState=ObjectState.Deleted;
            _constantTypeService.Delete(constantType);
            await _unitOfWorkAsync.SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }
        // GET: odata/ConstantTypes(5)/ConstantValues
        [EnableQuery]
        public IQueryable<ConstantValue> GetConstantValues([FromODataUri] long key)
        {
            return _constantTypeService.Queryable().Where(m => m.Id == key).SelectMany(m => m.ConstantValues);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool ConstantTypeExists(string constantTypeName)
        {
            return _constantTypeService.Query(e => e.ConstantTypeName == constantTypeName).Select().Any();
        }
        private bool ConstantTypeExists(long id)
        {
            return _constantTypeService.Query(e => e.Id == id).Select().Any();
        }
    }
}