using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNBillLogArchivesController : ODataController
    {
        private readonly ICNBillLogArchiveService _repo;
        public CNBillLogArchivesController(ICNBillLogArchiveService service)
        {
            _repo = service;
        }
        // GET: odata/CNBillLogArchives
        [HttpGet, EnableQuery]
        public IQueryable<CNBillLogArchive> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/CNBillLogArchives(5)
        [EnableQuery]
        public SingleResult<CNBillLogArchive> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/CNBillLogArchives(5)
        public async Task<IHttpActionResult> Put(long key, CNBillLogArchive CNBillLogArchive)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != CNBillLogArchive.Id)
            {
                return BadRequest();
            }
            CNBillLogArchive.ObjectState = ObjectState.Modified;
            _repo.Update(CNBillLogArchive);
            await Request.GetContext().SaveChangesAsync();

            return Updated(CNBillLogArchive);
        }
        // POST: odata/CNBillLogArchives
        public async Task<IHttpActionResult> Post(CNBillLogArchive CNBillLogArchive)
        {
            CNBillLogArchive.ObjectState = ObjectState.Added;
            CNBillLogArchive.BalanceAmount = CNBillLogArchive.TotalBillAmount;
            var ch = _repo.Insert(CNBillLogArchive);
            await Request.GetContext().SaveChangesAsync();
            return Created(ch);
        }
        

        //// PATCH: odata/CNBillLogArchives(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNBillLogArchive> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            CNBillLogArchive ch = await _repo.FindAsync(key);
            if (ch == null)
            {
                return NotFound();
            }
            var bill = await 
                Request.GetContext()
                    .RepositoryAsync<CNBill>()
                    .Queryable()
                    .Include(x => x.fk_BillNature.CNBillTypeId)
                    .Select(x=>new {x.fk_BillNature.CNBillTypeId,x.Id})
                    .FirstOrDefaultAsync(x => x.Id == ch.BillId);
            ch.ObjectState = ObjectState.Modified;
            patch.Patch(ch);
           await Request.GetContext().SaveChangesAsync();            
            return Updated(ch);
        }
        // DELETE: odata/CNBillLogArchives(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var CNBillLogArchive = await _repo.FindAsync(key);
            if (CNBillLogArchive == null)
            {
                return NotFound();
            }
            CNBillLogArchive.ObjectState = ObjectState.Deleted;
            _repo.Delete(CNBillLogArchive);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing&&!Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

    }
}