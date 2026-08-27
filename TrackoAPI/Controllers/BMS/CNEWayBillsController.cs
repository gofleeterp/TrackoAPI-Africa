using Repository.Pattern.Core.Repositories;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNEWayBillsController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<CNEWayBill> _repo;

        public CNEWayBillsController(IRepositoryAsync<CNEWayBill> service)
        {
            _repo = service;
        }
        // GET: odata/CNEWayBills
        [HttpGet, EnableQuery]
        public IQueryable<CNEWayBill> Get() => _repo.Queryable();

        // GET: odata/CNEWayBills(5)
        [EnableQuery]
        public SingleResult<CNEWayBill> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/CNEWayBills(5)
        public async Task<IHttpActionResult> Put(long key, CNEWayBill enitity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != enitity.Id)
            {
                return BadRequest();
            }
            enitity.ObjectState = ObjectState.Modified;
            _repo.Update(enitity);

            try
            {
                //  await _unitOfWorkAsync.SaveChangesAsync();
                await Request.GetContext().SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(enitity);
        }
        // POST: odata/CNEWayBills
        public async Task<IHttpActionResult> Post(CNEWayBill enitity)
        {
            enitity.ObjectState = ObjectState.Added;
            _repo.Insert(enitity);
            await Request.GetContext().SaveChangesAsync();
            return Created(enitity);
        }
        //// PATCH: odata/MaterialMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNEWayBill> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CNEWayBill enitity = await _repo.FindAsync(key);
            if (enitity == null)
            {
                return NotFound();
            }
            enitity.ObjectState = ObjectState.Modified;
            patch.Patch(enitity);
            try
            {
                _repo.Update(enitity);
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!Exists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(enitity);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var enitity = await _repo.FindAsync(key);
            if (enitity == null)
            {
                return NotFound();
            }
            
            enitity.ObjectState = ObjectState.Deleted;
            _repo.Delete(enitity);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && !Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool Exists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}