using System.Collections.Generic;
using System.Data.Entity;
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
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class SpareMastersController : ODataController
    //ODataController
    {
        private readonly ISpareMasterService _objSpareMasterService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public SpareMastersController(IUnitOfWorkAsync unitOfWorkAsync, ISpareMasterService service)
        {
            _objSpareMasterService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/SpareMasters
        [HttpGet, EnableQuery]
        public IQueryable<SpareMaster> Get()
        {
            return _objSpareMasterService.Queryable();
        }
        // GET: odata/SpareMasters(5)
        [EnableQuery]
        public SingleResult<SpareMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objSpareMasterService.Queryable().Where(t => t.Id == key));
        }
        [HttpPost, ODataRoute("AlterSparePartStatus")]
        public IHttpActionResult AlterSparePartStatus(ODataActionParameters parameters)
        {
            object idsObj;
            List<long> ids = new List<long>();
            if (parameters.TryGetValue("ids", out idsObj))
            {
                var str = idsObj as string;
                if (!string.IsNullOrWhiteSpace(str))
                {
                    foreach (string s in str.Split(','))
                    {
                        try
                        {
                            ids.Add(long.Parse(s));
                        }
                        catch
                        {
                            return BadRequest($"Unable to Cast {s}");
                        }

                    }
                }
            }
            if (ids.Count == 0)
            {
                return BadRequest("No Ids supplied");
            }
            _objSpareMasterService.AlterStatus(ids);
            if (_unitOfWorkAsync.SaveChanges() > 0)
            {
                return Ok();
            }
            return NotFound();
        }
        // PUT: odata/SpareMasters(5)
        public async Task<IHttpActionResult> Put(long key, SpareMaster objSpareMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objSpareMaster.Id)
            {
                return BadRequest();
            }
            objSpareMaster.ObjectState = ObjectState.Modified;
            _objSpareMasterService.Update(objSpareMaster);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpareMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objSpareMaster);
        }
        // POST: odata/SpareMasters
        public async Task<IHttpActionResult> Post(SpareMaster objSpareMaster)
        {
            objSpareMaster.ObjectState = ObjectState.Added;
            _objSpareMasterService.Insert(objSpareMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (SpareMasterExists(objSpareMaster.SpareName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objSpareMaster);
        }
        //// PATCH: odata/SpareMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<SpareMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            SpareMaster objSpareMaster = await _objSpareMasterService.FindAsync(key);
            if (objSpareMaster == null)
            {
                return NotFound();
            }
            objSpareMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objSpareMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SpareMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objSpareMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objSpareMaster = await _objSpareMasterService.FindAsync(key);
            if (objSpareMaster == null)
            {
                return NotFound();
            }
            objSpareMaster.ObjectState = ObjectState.Deleted;
            _objSpareMasterService.Delete(objSpareMaster);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        // POST: odata/VehicleMovementLogs(key)/Challans
        [ODataRoute("SpareMasters({key})/Aliases")]
        public async Task<IHttpActionResult> PostAliases([FromODataUri]long key, [FromBody] MasterAlias alias)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var ch = await _objSpareMasterService.Queryable().AnyAsync(x => x.Id == key);
            if (!ch)
            {
                return NotFound();
            }
            alias.SpareItemId = key;
            alias.ObjectState = ObjectState.Added;
            alias.RelatedTypeId = 1072;//ConstantValue
            alias.RelatedId = key;
            var item = uow.RepositoryAsync<MasterAlias>().Insert(alias);
            await uow.SaveChangesAsync();
            return Created(item);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        private bool SpareMasterExists(string spareName)
        {
            return _objSpareMasterService.Query(e => e.SpareName == spareName).Select().Any();
        }
        private bool SpareMasterExists(long key)
        {
            return _objSpareMasterService.Query(e => e.Id == key).Select().Any();
        }
    }
}