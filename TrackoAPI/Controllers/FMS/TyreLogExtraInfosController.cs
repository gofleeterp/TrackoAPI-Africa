using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using System.Windows.Forms;

using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Models.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Repository;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class TyreLogExtraInfosController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<TyreLogExtraInfo> _log;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public TyreLogExtraInfosController(IUnitOfWorkAsync unitOfWorkAsync)
        {
            _unitOfWorkAsync = unitOfWorkAsync;
            _log = unitOfWorkAsync.RepositoryAsync<TyreLogExtraInfo>();
        }
        // GET: odata/TyreLogs
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<TyreLogExtraInfo> Get()
        {
            return _log.Queryable();
        }
        // GET: odata/TyreLogs(5)
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<TyreLogExtraInfo> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_log.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/TyreLogs(5)
        public async Task<IHttpActionResult> Put(long key, TyreLogExtraInfo objTyreLog)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objTyreLog.Id)
            {
                return BadRequest();
            }
            objTyreLog.ObjectState = ObjectState.Modified;
            objTyreLog.ConstCurTypeId = Helper.ConstCurTypeId;
            _log.Update(objTyreLog);
            await _unitOfWorkAsync.SaveChangesAsync();

            return Updated(objTyreLog);
        }
        // POST: odata/TyreLogs
        public async Task<IHttpActionResult> Post(TyreLogExtraInfo objTyreLog)
        {
            objTyreLog.ObjectState = ObjectState.Added;
            objTyreLog.ConstCurTypeId = Helper.ConstCurTypeId;
            _log.Insert(objTyreLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return Created(objTyreLog);
        }
        //// PATCH: odata/TyreLogs(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TyreLogExtraInfo> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TyreLogExtraInfo objTyreLog = await _log.FindAsync(key);
            if (objTyreLog == null)
            {
                return NotFound();
            }
            objTyreLog.ObjectState = ObjectState.Modified;
            patch.Patch(objTyreLog);
            objTyreLog.ConstCurTypeId = Helper.ConstCurTypeId;
            await _unitOfWorkAsync.SaveChangesAsync();
            return Updated(objTyreLog);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objTyreLog = await _log.FindAsync(key);
            if (objTyreLog == null)
            {
                return NotFound();
            }
            objTyreLog.ObjectState = ObjectState.Deleted;
            _log.Delete(objTyreLog);
            await _unitOfWorkAsync.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _unitOfWorkAsync.Dispose();
            }
            base.Dispose(disposing);
        }

        
    }
}