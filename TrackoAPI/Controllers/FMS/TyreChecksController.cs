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
    public class TyreCheckingsController : ODataController
    //ODataController
    {
        private readonly ITyreCheckService _objTyreCheckService;

        public TyreCheckingsController(ITyreCheckService service)
        {
            _objTyreCheckService = service;
        }
        // GET: odata/TyreChecks
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<TyreCheck> Get()
        {
            return _objTyreCheckService.Queryable();
        }
        // GET: odata/TyreChecks(5)
        [EnableQuery]
        public SingleResult<TyreCheck> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objTyreCheckService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/TyreChecks(5)
        public async Task<IHttpActionResult> Put(long key, TyreCheck objTyreCheck)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTyreCheck.Id)
            {
                return BadRequest();
            }
            objTyreCheck.ObjectState = ObjectState.Modified;
            _objTyreCheckService.Update(objTyreCheck);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objTyreCheck);
        }
        //[ODataRoute("TyreCheckings({key})/JobCard/$ref"),HttpPost]
        //public IHttpActionResult LinkJobCard()
        //{

        //}
        [AcceptVerbs("POST", "PUT")]//For Single Navigation Properties
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty,
            [FromBody] Uri link)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var _uow = Request.GetContext();
            TyreCheck tyrecheck = await _objTyreCheckService.FindAsync(key);
            if (tyrecheck == null)
            {
                return NotFound();
            }
            long navigationkey = Request.GetKeyFromUri<long>(link);
            
            switch (navigationProperty)
            {
                case "JobCard":
                    VehicleMovementLog jobcard = await _uow.RepositoryAsync<VehicleMovementLog>().FindAsync(navigationkey);
                    if (jobcard == null)
                    {
                        return NotFound();
                    }
                    tyrecheck.JobCard = jobcard;
                    tyrecheck.JobCardId = jobcard.Id;
                    tyrecheck.ObjectState = ObjectState.Modified;
                    await _uow.SaveChangesAsync();
                    break;
                default:
                    return NotFound();
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("POST", "PUT")]//For Multiple Navigation Properties
        public async Task<IHttpActionResult> CreateLink([FromODataUri] int key, string navigationProperty,
            [FromBody] Uri link)
        {
            return StatusCode(HttpStatusCode.Forbidden);
        }

        // POST: odata/TyreChecks
        public async Task<IHttpActionResult> Post(TyreCheck objTyreCheck)
        {
            objTyreCheck.ObjectState = ObjectState.Added;
            _objTyreCheckService.Insert(objTyreCheck);
            await Request.GetContext().SaveChangesAsync();
            return Created(objTyreCheck);
        }
        //// PATCH: odata/TyreChecks(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TyreCheck> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TyreCheck objTyreCheck = await _objTyreCheckService.FindAsync(key);
            if (objTyreCheck == null)
            {
                return NotFound();
            }
            objTyreCheck.ObjectState = ObjectState.Modified;
            patch.Patch(objTyreCheck);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objTyreCheck);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTyreCheck = await _objTyreCheckService.FindAsync(key);
            if (objTyreCheck == null)
            {
                return NotFound();
            }
            objTyreCheck.ObjectState = ObjectState.Deleted;
            _objTyreCheckService.Delete(objTyreCheck);
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