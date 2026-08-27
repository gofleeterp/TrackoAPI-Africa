using System.Collections.Generic;
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
using System.Web.OData.Routing;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class DueTypesController : ODataController
    //ODataController
    {
        private readonly IDueMasterService _objDueMasterService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        public DueTypesController(IUnitOfWorkAsync unitOfWorkAsync, IDueMasterService service)
        {
            _objDueMasterService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/DueMasters
        [HttpGet, EnableQuery]
        public IQueryable<DueMaster> Get()
        {
            return _objDueMasterService.Queryable();
        }
        // GET: odata/DueMasters(5)
        [EnableQuery]
        public SingleResult<DueMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objDueMasterService.Queryable().Where(t => t.Id == key));
        }
        [HttpPost, ODataRoute("AlterDueTypesStatus")]
        public IHttpActionResult AlterDueTypesStatus(ODataActionParameters parameters)
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
            _objDueMasterService.AlterStatus(ids);
            if (_unitOfWorkAsync.SaveChanges() > 0)
            {
                return Ok();
            }
            return NotFound();
        }
        // GET: odata/DueMasters(5)/DueTypeId
        public IHttpActionResult GetDueTypeId(long key)
        {
            if (key == 0)
            {
                return NotFound();
            }
            var duetypeid =
                _objDueMasterService.Queryable().Where(x => x.Id == key).Select(x => x.DueTypeId).FirstOrDefault();
            if (duetypeid == 0)
            {
                return NotFound();
            }
            return Ok(duetypeid);
        }
        // PUT: odata/DueMasters(5)
        public async Task<IHttpActionResult> Put([FromODataUri] long key, DueMaster objDueMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objDueMaster.Id)
            {
                return BadRequest();
            }
            objDueMaster.ObjectState = ObjectState.Modified;
            _objDueMasterService.Update(objDueMaster);

            try
            {
                
                await _unitOfWorkAsync.SaveChangesAsync();
               
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DueMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDueMaster);
        }
        // POST: odata/DueMasters
        public async Task<IHttpActionResult> Post(DueMaster objDueMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            objDueMaster.ObjectState = ObjectState.Added;
            _objDueMasterService.Insert(objDueMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
                
            }
            catch (DbUpdateException)
            {
                if (DueMasterExists(objDueMaster.Name))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objDueMaster);
        }
        //// PATCH: odata/DueMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<DueMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            DueMaster objDueMaster = await _objDueMasterService.FindAsync(key);
            if (objDueMaster == null)
            {
                return NotFound();
            }
            objDueMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objDueMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
                
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!DueMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objDueMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objDueMaster = await _objDueMasterService.FindAsync(key);
            if (objDueMaster == null)
            {
                return NotFound();
            }
            objDueMaster.ObjectState = ObjectState.Deleted;
            _objDueMasterService.Delete(objDueMaster);
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

        private bool DueMasterExists(string dueName)
        {
            return _objDueMasterService.Query(e => e.Name == dueName).Select().Any();
        }
        private bool DueMasterExists(long key)
        {
            return _objDueMasterService.Query(e => e.Id == key).Select().Any();
        }
    }
}