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
    public class MaterialLocationMappingController : ODataController
    //ODataController
    {
        private readonly IMaterialLocationMappingService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public MaterialLocationMappingController(IUnitOfWorkAsync unitOfWorkAsync, IMaterialLocationMappingService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/MaterialLocationMapping
        [HttpGet, EnableQuery]
        public IQueryable<MaterialLocationMap> Get() => _repo.Queryable();

        // GET: odata/MaterialLocationMapping(5)
        [EnableQuery]
        public SingleResult<MaterialLocationMap> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/MaterialLocationMapping(5)
        public async Task<IHttpActionResult> Put(long key, MaterialLocationMap record)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != record.Id)
            {
                return BadRequest();
            }
            record.ObjectState = ObjectState.Modified;
            _repo.Update(record);

            try
            {
               await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialLocationMapExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(record);
        }
        // POST: odata/MaterialLocationMapping
        public async Task<IHttpActionResult> Post(MaterialLocationMap record)
        {
            record.ObjectState = ObjectState.Added;
            _repo.Insert(record);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MaterialLocationMapExists(record.MaterialId,record.PlantId,record.LocationId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(record);
        }
        //// PATCH: odata/MaterialLocationMapping(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<MaterialLocationMap> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MaterialLocationMap record = await _repo.FindAsync(key);
            if (record == null)
            {
                return NotFound();
            }
            record.ObjectState = ObjectState.Modified;
            patch.Patch(record);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialLocationMapExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(record);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var record = await _repo.FindAsync(key);
            if (record == null)
            {
                return NotFound();
            }
            record.ObjectState = ObjectState.Deleted;
            _repo.Delete(record);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        //[ODataRoute("MaterialLocationMapping({key})/MaterialParties")]
        //public async Task<IHttpActionResult> PostMaterialParties([FromODataUri]long key, [FromBody] MaterialParty map)
        //{
        //    if (!_repo.Queryable().Any(x => x.Id == key))
        //    {
        //        return NotFound();
        //    }
        //    if (map.PartyId.GetValueOrDefault() == 0)
        //    {
        //        return BadRequest("Party Required");
        //    }
        //    var mPrepo = Request.GetContext().RepositoryAsync<MaterialParty>();
        //    if (!ModelState.IsValid)
        //    {
        //        return BadRequest(ModelState);
        //    }
        //    var uow = Request.GetContext();
        //    map.MaterialId = key;
        //    map.ObjectState = ObjectState.Added;
        //    var item = mPrepo.Insert(map);
        //    await uow.SaveChangesAsync();
        //    return Created(item);
        //}
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MaterialLocationMapExists(long materialId,long plantId,long? locationid) => _repo.Query(e => e.MaterialId == materialId&&e.PlantId==plantId&&e.LocationId==locationid).Select().Any();
        private bool MaterialLocationMapExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}