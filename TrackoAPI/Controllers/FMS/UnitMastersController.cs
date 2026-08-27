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
    public class UnitMastersController : ODataController
    //ODataController
    {
        private readonly IUnitMasterService _um;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public UnitMastersController(IUnitOfWorkAsync unitOfWorkAsync, IUnitMasterService service)
        {
            _um = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/UnitMasters
        [HttpGet, EnableQuery]
        public IQueryable<UnitMaster> Get()
        {
            return _um.Queryable();
        }
        // GET: odata/UnitMasters(5)
        [EnableQuery]
        public SingleResult<UnitMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_um.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/UnitMasters(5)
        public async Task<IHttpActionResult> Put(long key, UnitMaster objUnitMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objUnitMaster.Id)
            {
                return BadRequest();
            }
            objUnitMaster.ObjectState = ObjectState.Modified;
            _um.Update(objUnitMaster);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objUnitMaster);
        }
        // POST: odata/UnitMasters
        public async Task<IHttpActionResult> Post(UnitMaster objUnitMaster)
        {
            objUnitMaster.ObjectState = ObjectState.Added;
            _um.Insert(objUnitMaster);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objUnitMaster);
        }
        //// PATCH: odata/UnitMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<UnitMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            UnitMaster objUnitMaster = await _um.FindAsync(key);
            if (objUnitMaster == null)
            {
                return NotFound();
            }
            objUnitMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objUnitMaster);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objUnitMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objUnitMaster = await _um.FindAsync(key);
            if (objUnitMaster == null)
            {
                return NotFound();
            }
            objUnitMaster.ObjectState = ObjectState.Deleted;
            _um.Delete(objUnitMaster);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        //protected override void Dispose(bool disposing)
        //{
        //    if (disposing)
        //    {
        //        _unitOfWorkAsync.Dispose();
        //    }
        //    base.Dispose(disposing);
        //}
    }
}