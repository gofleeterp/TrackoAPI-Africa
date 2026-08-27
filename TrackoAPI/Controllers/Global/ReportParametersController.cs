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
    public class ReportParametersController : ODataController
    //ODataController
    {
        private readonly IReportParameterService _service;

        public ReportParametersController(IReportParameterService service)
        {
            _service = service;
        }
        // GET: odata/CityMasters
        [HttpGet, EnableQuery]
        public IQueryable<ReportParameter> Get()
        {
            return _service.Queryable();
        }

        // GET: odata/CityMasters(5)
        [EnableQuery]
        public SingleResult<ReportParameter> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/CityMasters(5)
        public async Task<IHttpActionResult> Put(long key, ReportParameter objReportParam)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != objReportParam.Id)
            {
                return BadRequest();
            }
            objReportParam.ObjectState = ObjectState.Modified;
            _service.Update(objReportParam);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objReportParam);
        }
        // POST: odata/CityMasters
        public async Task<IHttpActionResult> Post(ReportParameter objReportParam)
        {
            objReportParam.ObjectState = ObjectState.Added;
            _service.Insert(objReportParam);
            await Request.GetContext().SaveChangesAsync();
            return Created(objReportParam);
        }
        //// PATCH: odata/CityMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ReportParameter> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ReportParameter objReportParam = await _service.FindAsync(key);
            if (objReportParam == null)
            {
                return NotFound();
            }
            objReportParam.ObjectState = ObjectState.Modified;
            patch.Patch(objReportParam);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objReportParam);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objReportParam = await _service.FindAsync(key);
            if (objReportParam == null)
            {
                return NotFound();
            }
            objReportParam.ObjectState = ObjectState.Deleted;
            _service.Delete(objReportParam);
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