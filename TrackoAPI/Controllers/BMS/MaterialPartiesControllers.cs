using System;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class MaterialPartiesController : ODataController
    //ODataController
    {
        private readonly IMaterialPartyService _repo;

        public MaterialPartiesController(IMaterialPartyService service)
        {
            _repo = service;
        }
        // GET: odata/MaterialParties
        [HttpGet, EnableQuery]
        public IQueryable<MaterialParty> Get() => _repo.Queryable();

        // GET: odata/MaterialParties(5)
        [EnableQuery]
        public SingleResult<MaterialParty> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/MaterialParties(5)
        public async Task<IHttpActionResult> Put(long key, MaterialParty objMaterialParty)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objMaterialParty.Id)
            {
                return BadRequest();
            }
            objMaterialParty.ObjectState = ObjectState.Modified;
            _repo.Update(objMaterialParty);

            try
            {
                await Request.GetContext().SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialPartyExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialParty);
        }
        // POST: odata/MaterialParties
        public async Task<IHttpActionResult> Post(MaterialParty objMaterialParty)
        {
            objMaterialParty.ObjectState = ObjectState.Added;
            _repo.Insert(objMaterialParty);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (MaterialPartyExists(objMaterialParty.MaterialId,objMaterialParty.PartyId))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Duplicate mapping found.");
                }
                throw;
            }
            return Created(objMaterialParty);
        }
        //// PATCH: odata/MaterialParties(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<MaterialParty> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MaterialParty objMaterialParty = await _repo.FindAsync(key);
            if (objMaterialParty == null)
            {
                return NotFound();
            }
            objMaterialParty.ObjectState = ObjectState.Modified;
            patch.Patch(objMaterialParty);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!MaterialPartyExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objMaterialParty);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objMaterialParty = await _repo.FindAsync(key);
            if (objMaterialParty == null)
            {
                return NotFound();
            }
            objMaterialParty.ObjectState = ObjectState.Deleted;
            _repo.Delete(objMaterialParty);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing &&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool MaterialPartyExists(long? materialId,long? partyId) => _repo.Query(e => e.MaterialId == materialId&&e.PartyId==partyId).Select().Any();
        private bool MaterialPartyExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}