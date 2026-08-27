using System;
using System.Data.Entity.Infrastructure;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Hangfire;
using Newtonsoft.Json;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.vw.ts;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class MaterialMastersController : ODataController
    //ODataController
    {
        private readonly IMaterialMasterService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public MaterialMastersController(IUnitOfWorkAsync unitOfWorkAsync, IMaterialMasterService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/MaterialMasters
        [HttpGet, EnableQuery]
        public IQueryable<MaterialMaster> Get() => _repo.Queryable();

        // GET: odata/MaterialMasters(5)
        [EnableQuery]
        public SingleResult<MaterialMaster> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/MaterialMasters(5)
        public async Task<IHttpActionResult> Put(long key, MaterialMaster objMaterialMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objMaterialMaster.Id)
            {
                return BadRequest();
            }
            objMaterialMaster.ObjectState = ObjectState.Modified;
            _repo.Update(objMaterialMaster);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
                var err = MaterialMasterPostProcess(objMaterialMaster, _unitOfWorkAsync,"update");
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialMaster);
        }
        private string MaterialMasterPostProcess(MaterialMaster _record, IUnitOfWorkAsync _uow,string _action)
        {
            var livevalidationerr = _uow.SqlQueryAsync(
            "[dbo].[Proc_TRANS_RunMaterialMasterPostProcess]",
            new SqlParameter() { Value = Helper.SessionId(), ParameterName = "parameter1" },/*SessionId*/
            new SqlParameter() { Value = _action, ParameterName = "parameter2" },/*action*/
            new SqlParameter() { Value = JsonConvert.SerializeObject(_record), ParameterName = "parameter3" }/*model*/
            ).Result;

            if (livevalidationerr != null && livevalidationerr?.Rows?.Count > 0)
            {
                return Utilities.To<string>(livevalidationerr.Rows[0]["ErrorMessage"]);
            }
            return "";
        }
        // POST: odata/MaterialMasters
        public async Task<IHttpActionResult> Post(MaterialMaster objMaterialMaster)
        {
            objMaterialMaster.ObjectState = ObjectState.Added;
            _repo.Insert(objMaterialMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
                var err = MaterialMasterPostProcess(objMaterialMaster, _unitOfWorkAsync,"add");
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
            }
            catch (DbUpdateException)
            {
                if (MaterialMasterExists(objMaterialMaster.MaterialName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objMaterialMaster);
        }
        //// PATCH: odata/MaterialMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<MaterialMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MaterialMaster objMaterialMaster = await _repo.FindAsync(key);
            if (objMaterialMaster == null)
            {
                return NotFound();
            }
            objMaterialMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objMaterialMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
                var err = MaterialMasterPostProcess(objMaterialMaster, _unitOfWorkAsync,"add");
                if (!string.IsNullOrWhiteSpace(err))
                {
                    return BadRequest(err);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objMaterialMaster = await _repo.FindAsync(key);
            if (objMaterialMaster == null)
            {
                return NotFound();
            }
            objMaterialMaster.ObjectState = ObjectState.Deleted;
            _repo.Delete(objMaterialMaster);
            await _unitOfWorkAsync.SaveChangesAsync();
            var err = MaterialMasterPostProcess(objMaterialMaster, _unitOfWorkAsync,"delete");
            if (!string.IsNullOrWhiteSpace(err))
            {
                return BadRequest(err);
            }
            return StatusCode(HttpStatusCode.NoContent);
        }
        //[ODataRoute("MaterialMasters({key})/MaterialParties")]
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

        private bool MaterialMasterExists(string materialName) => _repo.Query(e => e.MaterialName == materialName).Select().Any();
        private bool MaterialMasterExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}