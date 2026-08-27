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
    public class ConstantValuesController:ODataController
    {
        private readonly IConstantValueService _constantValueService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public ConstantValuesController(IUnitOfWorkAsync unitOfWorkAsync, IConstantValueService service)
        {
            _constantValueService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/ConstantValues
        [HttpGet,EnableQuery]
        public IQueryable<ConstantValue> GetConstantValues()
        {
            return _constantValueService.Queryable();
        }
        // GET: odata/ConstantValues(5)
        [EnableQuery]
        public SingleResult<ConstantValue> GetConstantValue([FromODataUri] long key)
        {
            return SingleResult.Create(_constantValueService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/ConstantValues(5)
       public async Task<IHttpActionResult> Put([FromODataUri]long key, ConstantValue constantValue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != constantValue.Id)
            {
                return BadRequest();
            }
            constantValue.ObjectState=ObjectState.Modified;
            _constantValueService.Update(constantValue);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConstantValueExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(constantValue);
        }
        // POST: odata/ConstantValues
        public async Task<IHttpActionResult> Post(ConstantValue constantValue)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            constantValue.ObjectState = ObjectState.Added;
            _constantValueService.Insert(constantValue);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ConstantValueExists(constantValue.ConstantName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(constantValue);
        }
        //// PATCH: odata/ConstantValues(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ConstantValue> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ConstantValue constantValue = await _constantValueService.FindAsync(key);

            if (constantValue == null)
            {
                return NotFound();
            }
            constantValue.ObjectState=ObjectState.Modified;
            patch.Patch(constantValue);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ConstantValueExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(constantValue);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            ConstantValue constantValue = await _constantValueService.FindAsync(key);

            if (constantValue == null)
            {
                return NotFound();
            }
            constantValue.ObjectState=ObjectState.Deleted;
            _constantValueService.Delete(constantValue);
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

        private bool ConstantValueExists(string constantValueName)
        {
            return _constantValueService.Query(e => e.ConstantName == constantValueName).Select().Any();
        }
        private bool ConstantValueExists(long id)
        {
            return _constantValueService.Query(e => e.Id == id).Select().Any();
        }
    }
}