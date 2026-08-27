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
    public class CNBillLogsController : ODataController
    {
        private readonly ICNBillLogService _repo;
        public CNBillLogsController(ICNBillLogService service)
        {
            _repo = service;
        }
        // GET: odata/CNBillLogs
        [HttpGet, EnableQuery]
        public IQueryable<CNBillLog> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/CNBillLogs(5)
        [EnableQuery]
        public SingleResult<CNBillLog> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/CNBillLogs(5)
        public async Task<IHttpActionResult> Put(long key, CNBillLog cnBillLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != cnBillLog.Id)
            {
                return BadRequest();
            }
            cnBillLog.ObjectState = ObjectState.Modified;
            _repo.Update(cnBillLog);
            await Request.GetContext().SaveChangesAsync();

            return Updated(cnBillLog);
        }
        // POST: odata/CNBillLogs
        public async Task<IHttpActionResult> Post(CNBillLog CNBillLog)
        {
            CNBillLog.ObjectState = ObjectState.Added;
            CNBillLog.BalanceAmount = CNBillLog.TotalBillAmount;
            var ch = _repo.Insert(CNBillLog);
            await Request.GetContext().SaveChangesAsync();
            return Created(ch);
        }
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var cn = _repo.Queryable().SingleOrDefault(p => p.Id == key);
            if (cn == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_CN":
                    cn.CNId = null;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
            return StatusCode(HttpStatusCode.NoContent);
        }

        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var bl = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (bl == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_CN":
                    var billrepo = uow.RepositoryAsync<CNMaster>();
                    var cn =
                        await
                            billrepo.Queryable().AnyAsync(x => x.Id == id);
                    if (!cn)
                    {
                        return NotFound();
                    }
                    bl.CNId = id;
                    bl.ObjectState = ObjectState.Modified;
                    await uow.SaveChangesAsync();
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        //// PATCH: odata/CNBillLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNBillLog> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            CNBillLog ch = await _repo.FindAsync(key);
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
            var count=await Request.GetContext().SaveChangesAsync();
            if (count > 0)
            {
                if (bill.CNBillTypeId == 1363&&ch.CNId>0)
                {
                    //var cn =await uow.RepositoryAsync<CNMaster>().FindAsync(billlog.CNId);
                    //cn.BillId = key;
                    //cn.ObjectState=ObjectState.Modified;
                    var result =
                        _repo.ExecuteSql($"UPDATE [dbo].[tCNMaster] SET [BillId] = {ch.BillId} WHERE  [Id] = {ch.CNId}");
                    if (result <= 0)
                    {
                        return BadRequest("Invalid CN in Bill");
                    }
                }
            }
            else
            {
                return BadRequest("Invalid CN in Bill");
            }
            return Updated(ch);
        }
        // DELETE: odata/CNBillLogs(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var cnBillLog = await _repo.FindAsync(key);
            if (cnBillLog == null)
            {
                return NotFound();
            }
            if (cnBillLog.CNId > 0)
            {
                var result =
                _repo.ExecuteSql($"UPDATE [dbo].[tCNMaster] SET [BillId] = NULL WHERE  [Id] = {cnBillLog.CNId}");
                if (result <= 0)
                {
                    return BadRequest("Invalid CN in Bill");
                }
            }
            cnBillLog.ObjectState = ObjectState.Deleted;
            _repo.Delete(cnBillLog);
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