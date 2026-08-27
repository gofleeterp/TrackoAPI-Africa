using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TripExpenseBudgetsController : ODataController
    //ODataController
    {
        private readonly ITripExpenseBudgetService _objTripExpenseBudgetService;

        public TripExpenseBudgetsController(ITripExpenseBudgetService service)
        {
            _objTripExpenseBudgetService = service;
        }
        
        [HttpGet, EnableQuery]
        public IQueryable<TripExpenseBudget> Get()
        {
            return _objTripExpenseBudgetService.Queryable();
        }
        // GET: odata/SpareAliass(5)
        [EnableQuery]
        public SingleResult<TripExpenseBudget> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objTripExpenseBudgetService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/SpareAliass(5)
        public async Task<IHttpActionResult> Put(long key, TripExpenseBudget objTripExpenseBudget)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTripExpenseBudget.Id)
            {
                return BadRequest();
            }
            objTripExpenseBudget.ObjectState = ObjectState.Modified;
            _objTripExpenseBudgetService.Update(objTripExpenseBudget);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objTripExpenseBudget);
        }
        // POST: odata/SpareAliass
        public async Task<IHttpActionResult> Post(TripExpenseBudget objTripExpenseBudget)
        {
            objTripExpenseBudget.ObjectState = ObjectState.Added;
            _objTripExpenseBudgetService.Insert(objTripExpenseBudget);
            await Request.GetContext().SaveChangesAsync();
            return Created(objTripExpenseBudget);
        }
        //// PATCH: odata/SpareAliass(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TripExpenseBudget> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TripExpenseBudget objTripExpenseBudget = await _objTripExpenseBudgetService.FindAsync(key);
            if (objTripExpenseBudget == null)
            {
                return NotFound();
            }
            objTripExpenseBudget.ObjectState = ObjectState.Modified;
            patch.Patch(objTripExpenseBudget);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objTripExpenseBudget);
        }
        
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objSpareAlias = await _objTripExpenseBudgetService.FindAsync(key);
            if (objSpareAlias == null)
            {
                return NotFound();
            }
            objSpareAlias.ObjectState = ObjectState.Deleted;
            _objTripExpenseBudgetService.Delete(objSpareAlias);
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