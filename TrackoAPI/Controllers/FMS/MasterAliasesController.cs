using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class MasterAliasesController : ODataController
    //ODataController
    {
        private readonly IMasterAliasService _objMasterAliasService;

        public MasterAliasesController(IMasterAliasService service)
        {
            _objMasterAliasService = service;
        }
        // GET: odata/SpareAliass
        [HttpGet, EnableQuery]
        public IQueryable<MasterAlias> Get()
        {
            return _objMasterAliasService.Queryable();
        }
        // GET: odata/SpareAliass(5)
        [EnableQuery]
        public SingleResult<MasterAlias> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objMasterAliasService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/SpareAliass(5)
        public async Task<IHttpActionResult> Put(long key, MasterAlias objMasterAlias)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objMasterAlias.Id)
            {
                return BadRequest();
            }
            objMasterAlias.ObjectState = ObjectState.Modified;
            _objMasterAliasService.Update(objMasterAlias);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objMasterAlias);
        }
        // POST: odata/SpareAliass
        public async Task<IHttpActionResult> Post(MasterAlias objMasterAlias)
        {
            objMasterAlias.ObjectState = ObjectState.Added;
            _objMasterAliasService.Insert(objMasterAlias);
            await Request.GetContext().SaveChangesAsync();
            return Created(objMasterAlias);
        }
        //// PATCH: odata/SpareAliass(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<MasterAlias> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            MasterAlias objMasterAlias = await _objMasterAliasService.FindAsync(key);
            if (objMasterAlias == null)
            {
                return NotFound();
            }
            objMasterAlias.ObjectState = ObjectState.Modified;
            patch.Patch(objMasterAlias);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objMasterAlias);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objSpareAlias = await _objMasterAliasService.FindAsync(key);
            if (objSpareAlias == null)
            {
                return NotFound();
            }
            objSpareAlias.ObjectState = ObjectState.Deleted;
            _objMasterAliasService.Delete(objSpareAlias);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
            }
            base.Dispose(disposing);
        }
        
    }
}