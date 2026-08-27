
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
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class MaterialGroupsController : ODataController
    //ODataController
    {
        private readonly IMaterialGroupService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public MaterialGroupsController(IUnitOfWorkAsync unitOfWorkAsync, IMaterialGroupService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/MaterialGroups
        [HttpGet, EnableQuery]
        public IQueryable<MaterialGroup> Get() => _repo.Queryable();

        // GET: odata/MaterialGroups(5)
        [EnableQuery]
        public SingleResult<MaterialGroup> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/MaterialGroups(5)
        public async Task<IHttpActionResult> Put(long key, MaterialGroup objMaterialGroup)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objMaterialGroup.Id)
            {
                return BadRequest();
            }
            objMaterialGroup.ObjectState = ObjectState.Modified;
            _repo.Update(objMaterialGroup);

            try
            {
              await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialGroupExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialGroup);
        }
        // POST: odata/MaterialGroups
        public async Task<IHttpActionResult> Post(MaterialGroup objMaterialGroup)
        {
            objMaterialGroup.ObjectState = ObjectState.Added;
            _repo.Insert(objMaterialGroup);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MaterialGroupExists(objMaterialGroup.Name, objMaterialGroup.Code))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name or Code should be unique");
                }
                throw;
            }
            return Created(objMaterialGroup);
        }
        //// PATCH: odata/MaterialGroups(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<MaterialGroup> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MaterialGroup objMaterialGroup = await _repo.FindAsync(key);
            if (objMaterialGroup == null)
            {
                return NotFound();
            }
            objMaterialGroup.ObjectState = ObjectState.Modified;
            patch.Patch(objMaterialGroup);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialGroupExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialGroup);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objMaterialGroup = await _repo.FindAsync(key);
            if (objMaterialGroup == null)
            {
                return NotFound();
            }
            objMaterialGroup.ObjectState = ObjectState.Deleted;
            _repo.Delete(objMaterialGroup);
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

        private bool MaterialGroupExists(string name,string code) => _repo.Query(e => (e.Name == name)|| (e.Code== code)).Select().Any();
        private bool MaterialGroupExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}