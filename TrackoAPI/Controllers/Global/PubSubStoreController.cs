using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;
using EntityFramework.Extensions;
using System.Collections.Generic;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class PubSubStoreController : ODataController
    //ODataController
    {
        private readonly IApiPubSubStoreService _objApiPubSubStoreervice;

        public PubSubStoreController(IApiPubSubStoreService service)
        {
            _objApiPubSubStoreervice = service;
        }
        // GET: odata/ApiPubSubStore
        [HttpGet, EnableQuery]
        public IQueryable<ApiPubSubStore> Get()
        {
            var userid = Helper.GetLoggedInUserId();
            return _objApiPubSubStoreervice.Queryable().Where(x=>x.ReceiverId== userid);
        }
        
        // GET: odata/ApiPubSubStore(5)
        [EnableQuery]
        public SingleResult<ApiPubSubStore> Get([FromODataUri] long key)
        {
            var userid = Helper.GetLoggedInUserId();
            return SingleResult.Create(_objApiPubSubStoreervice.Queryable().Where(t => t.Id == key&&t.ReceiverId== userid));
        }
        [HttpPost]
        public async Task<IHttpActionResult> AcknolodgeAllMessages(ODataActionParameters parameters)
        {
            var typeId =(long)parameters["typeId"];
            var ids = parameters["ids"] as IEnumerator<long>;
            if (ids == null) return BadRequest("No Record found to remove");
            var cns = ids.ToList();
            var userid = Helper.GetLoggedInUserId();            
            var count=await _objApiPubSubStoreervice.Queryable().Where(x => x.ReceiverId == userid&&x.RecordTypeId==typeId&&cns.Contains(x.Id)).DeleteAsync();
            if (count > 0)
            {
                return Ok();
            }
            else
            {
                return BadRequest("Unable to Delete Logs");
            }
        }
        // PUT: odata/ApiPubSubStore(5)
        public async Task<IHttpActionResult> Put(long key, ApiPubSubStore objApiPubSubStore)
        {
            return Unauthorized();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objApiPubSubStore.Id)
            {
                return BadRequest();
            }
            _objApiPubSubStoreervice.Update(objApiPubSubStore);

            try
            {
                await Request.GetContext().SaveChangesAsync();
                
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApiPubSubStoreExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objApiPubSubStore);
        }
        // POST: odata/ApiPubSubStore
        public async Task<IHttpActionResult> Post(ApiPubSubStore objApiPubSubStore)
        {
            
            _objApiPubSubStoreervice.Insert(objApiPubSubStore);
            await Request.GetContext().SaveChangesAsync();
            await Request.GetHubContext().SyncTransaction(objApiPubSubStore);
            return Created(objApiPubSubStore);
        }
        //// PATCH: odata/ApiPubSubStore(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ApiPubSubStore> patch)
        {
            return Unauthorized();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ApiPubSubStore objApiPubSubStore = await _objApiPubSubStoreervice.FindAsync(key);
            if (objApiPubSubStore == null)
            {
                return NotFound();
            }
            patch.Patch(objApiPubSubStore);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ApiPubSubStoreExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objApiPubSubStore);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objApiPubSubStore = await _objApiPubSubStoreervice.FindAsync(key);
            if(objApiPubSubStore.ReceiverId!=Helper.GetLoggedInUserId()) return Unauthorized();
            if (objApiPubSubStore == null)
            {
                return NotFound();
            }
            _objApiPubSubStoreervice.Delete(objApiPubSubStore);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
        
        private bool ApiPubSubStoreExists(long key)
        {
            return _objApiPubSubStoreervice.Query(e => e.Id == key).Select().Any();
        }
    }
}