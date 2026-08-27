using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CardsController : ODataController
    //ODataController
    {
        private readonly ICardMasterService _objCardMasterService;

        public CardsController(ICardMasterService service)
        {
            _objCardMasterService = service;
        }
        // GET: odata/Cards
        [HttpGet, EnableQuery]
        public IQueryable<CardMaster> Get()
        {
            return _objCardMasterService.Queryable();
        }
        // GET: odata/Cards(5)
        [EnableQuery]
        public SingleResult<CardMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objCardMasterService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/Cards(5)
        public async Task<IHttpActionResult> Put(long key, CardMaster objCardMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objCardMaster.Id)
            {
                return BadRequest();
            }
            objCardMaster.ObjectState = ObjectState.Modified;
            _objCardMasterService.Update(objCardMaster);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objCardMaster);
        }
        // POST: odata/CardMasters
        public async Task<IHttpActionResult> Post(CardMaster objCardMaster)
        {
            objCardMaster.ObjectState = ObjectState.Added;
            _objCardMasterService.Insert(objCardMaster);
            await Request.GetContext().SaveChangesAsync();
            return Created(objCardMaster);
        }
        //// PATCH: odata/CardMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CardMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CardMaster objCardMaster = await _objCardMasterService.FindAsync(key);
            if (objCardMaster == null)
            {
                return NotFound();
            }
            objCardMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objCardMaster);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objCardMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objCardMaster = await _objCardMasterService.FindAsync(key);
            if (objCardMaster == null)
            {
                return NotFound();
            }
            objCardMaster.ObjectState = ObjectState.Deleted;
            _objCardMasterService.Delete(objCardMaster);
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