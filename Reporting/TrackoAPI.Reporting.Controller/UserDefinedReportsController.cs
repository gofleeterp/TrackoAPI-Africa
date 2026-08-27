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
    public class UserDefinedReportsController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<UserDefinedReport> _repo;

        public UserDefinedReportsController(IRepositoryAsync<UserDefinedReport> service)
        {
            _repo = service;
        }
        // GET: odata/CityMasters
        [HttpGet, EnableQuery]
        public IQueryable<UserDefinedReport> Get()
        {
            return _repo.Queryable();
        }
        
        // GET: odata/CityMasters(5)
        [EnableQuery]
        public SingleResult<UserDefinedReport> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/CityMasters(5)
        public async Task<IHttpActionResult> Put(long key, UserDefinedReport report)
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
                throw;
            }

            return Updated(report);
        }

        private bool ReportExists(long key)
        {
            return _repo.Queryable().Any(x => x.Id == key);
        }

        // POST: odata/CityMasters
        public async Task<IHttpActionResult> Post(UserDefinedReport report)
        {            
            var mainReport =await
                _repo.GetRepository<ReportProcedure>()
                    .Queryable()
                    .Include(x => x.fk_Report)
                    .FirstOrDefaultAsync(x => x.ReportId == report.ParentReportId);
            
            if (mainReport == null)
            {
                return BadRequest("Parent Report Not Found");
            }
            var parentParameters = await
                _repo.GetRepository<ReportParameter>()
                    .Queryable()
                    .Where(x => x.ReportId == report.ParentReportId).ToListAsync();
            var proc=new UserDefinedReportProcedure()
            {
                Columns = mainReport.Columns,
                ObjectState = ObjectState.Added,
                StoredProcedureName = mainReport.StoredProcedureName,
                UsaseCount = 0,
                UserDefinedReportId = report.Id,
                fk_Report = report
            };
            _repo.GetRepository<UserDefinedReportProcedure>().Insert(proc);
            report.ObjectState = ObjectState.Added;
            if (parentParameters.Any())
            {
                foreach (var param in parentParameters)
                {
                    report.Parameters.Add(new UserDefinedReportParameter
                    {
                        ObjectState=ObjectState.Added,
                        ReportId = report.Id,
                        fk_Report = report,
                        ParameterId =param.ParameterId,
                        FieldTypeId=param.FieldTypeId,
                        ParameterCaption=param.ParameterCaption,
                        EnumTypeId=param.EnumTypeId,
                        RoleTypeId=param.RoleTypeId,
                        RoleIds=param.RoleIds,
                        IsRequired=param.IsRequired,
                        CustomDataSource=param.CustomDataSource,
                        ProcParamName=param.ProcParamName,
                    });
                }                
            }
            _repo.Insert(report);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ReportExists(report.Name))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(report);
        }

        private bool ReportExists(string name)
        {
            return _repo.Queryable().Any(x => x.Name == name);
        }

        //// PATCH: odata/CityMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<UserDefinedReport> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var objCityMaster = await _repo.FindAsync(key);
            if (objCityMaster == null)
            {
                return NotFound();
            }
            objCityMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objCityMaster);
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
                throw;
            }

            return Updated(objCityMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objCityMaster = await _repo.FindAsync(key);
            if (objCityMaster == null)
            {
                return NotFound();
            }
            objCityMaster.ObjectState = ObjectState.Deleted;
            _repo.Delete(objCityMaster);
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