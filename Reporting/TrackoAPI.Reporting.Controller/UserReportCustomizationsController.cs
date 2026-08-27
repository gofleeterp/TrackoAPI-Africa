using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Reporting.Controller
{
    [AuthorizeEx]
    public class UserReportCustomizationsController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<UserReportCustomization> _repo;

        public UserReportCustomizationsController(IRepositoryAsync<UserReportCustomization> service)
        {
            _repo = service;
        }
        // GET: odata/UserReportCustomizations
        [HttpGet, EnableQuery]
        public IQueryable<UserReportCustomization> Get()
        {
            return _repo.Queryable();
        }
        
        // GET: odata/UserReportCustomizations(5)
        [EnableQuery]
        public SingleResult<UserReportCustomization> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/UserReportCustomizations(5)
        public async Task<IHttpActionResult> Put(long key, UserReportCustomization report)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != report.Id)
            {
                return BadRequest();
            }
            report.ObjectState = ObjectState.Modified;
            _repo.Update(report);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(key))
                {
                    return NotFound();
                }
                if (IsDuplicate(report.ReportId,report.UserDefinedReportId,report.UserId,report.ReportName))
                {
                    return BadRequest("Customization Already Exists");
                }
                throw;
            }

            return Updated(report);
        }

        private bool IsDuplicate(long? reportid,long? userdefinedReportId, long? userid,string reportName)
        {
            return _repo.Queryable().Any(x => x.ReportId == reportid&&x.UserDefinedReportId==userdefinedReportId&&userid==x.UserId&&reportName==x.ReportName);
        }

        // POST: odata/UserReportCustomizations
        public async Task<IHttpActionResult> Post(UserReportCustomization report)
        {
            report.ObjectState = ObjectState.Added;
            _repo.Insert(report);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (IsDuplicate(report.ReportId, report.UserDefinedReportId, report.UserId,report.ReportName))
                {
                    return BadRequest("Customization Already Exists");
                }
                throw;
            }
            return Created(report);
        }

        private bool ReportExists(long key)
        {
            return _repo.Queryable().Any(x => x.Id == key);
        }

        //// PATCH: odata/UserReportCustomizations(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<UserReportCustomization> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var report = await _repo.FindAsync(key);
            if (report == null)
            {
                return NotFound();
            }
            report.ObjectState = ObjectState.Modified;
            patch.Patch(report);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ReportExists(key))
                {
                    return NotFound();
                }
                if (IsDuplicate(report.ReportId, report.UserDefinedReportId, report.UserId,report.ReportName))
                {
                    return BadRequest("Customization Already Exists");
                }
                throw;
            }

            return Updated(report);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var report = await _repo.FindAsync(key);
            if (report == null)
            {
                return NotFound();
            }
            report.ObjectState = ObjectState.Deleted;
            _repo.Delete(report);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing&& !Request.IsBatchRequest())
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}