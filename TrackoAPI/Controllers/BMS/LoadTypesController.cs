
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class LoadTypesController : ODataController
    //ODataController
    {
        private readonly ILoadTypeService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public LoadTypesController(IUnitOfWorkAsync unitOfWorkAsync, ILoadTypeService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/LoadTypes
        [HttpGet, EnableQuery]
        public IQueryable<LoadType> Get() => _repo.Queryable();

        // GET: odata/LoadTypes(5)
        [EnableQuery]
        public SingleResult<LoadType> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/LoadTypes(5)
        public async Task<IHttpActionResult> Put(long key, LoadType objLoadType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objLoadType.Id)
            {
                return BadRequest();
            }
            objLoadType.ObjectState = ObjectState.Modified;
            _repo.Update(objLoadType);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoadTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLoadType);
        }
        // POST: odata/LoadTypes
        public async Task<IHttpActionResult> Post(LoadType objLoadType)
        {
            objLoadType.ObjectState = ObjectState.Added;
            _repo.Insert(objLoadType);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (LoadTypeExists(objLoadType.Name, objLoadType.Code))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name or Code should be unique");
                }
                throw;
            }
            return Created(objLoadType);
        }
        //// PATCH: odata/LoadTypes(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<LoadType> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            LoadType objLoadType = await _repo.FindAsync(key);
            if (objLoadType == null)
            {
                return NotFound();
            }
            objLoadType.ObjectState = ObjectState.Modified;
            patch.Patch(objLoadType);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LoadTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objLoadType);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objLoadType = await _repo.FindAsync(key);
            if (objLoadType == null)
            {
                return NotFound();
            }
            objLoadType.ObjectState = ObjectState.Deleted;
            _repo.Delete(objLoadType);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        public async Task<long?> GetRateCriteriaId([FromODataUri] long key)
        {
            return await _repo.Queryable().Where(x => x.Id == key).Select(x => x.RateCriteriaId).FirstOrDefaultAsync();
        }
        public async Task<string> GetScript([FromODataUri] long key)
        {
            var scriptId= await _repo.Queryable().Where(x => x.Id == key&&x.ScriptId!=null).Select(x => x.ScriptId).FirstOrDefaultAsync();
            return await 
                Request.GetContext()
                    .RepositoryAsync<ApiWorkFlowScript>()
                    .Queryable()
                    .Where(x => x.Id == scriptId.Value)
                    .Select(x => x.Script)
                    .FirstOrDefaultAsync();

        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool LoadTypeExists(string name,string code) => _repo.Query(e => (e.Name == name)|| (e.Code== code)).Select().Any();
        private bool LoadTypeExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}