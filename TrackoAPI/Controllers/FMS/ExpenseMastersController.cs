using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.FMS;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ExpenseMastersController : ODataController
    //ODataController
    {
        private readonly IExpenseMasterService _objExpenseMasterService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public ExpenseMastersController(IUnitOfWorkAsync unitOfWorkAsync, IExpenseMasterService service)
        {
            _objExpenseMasterService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/ExpenseMasters
        [HttpGet, EnableQuery]
        public IQueryable<ExpenseMaster> Get()
        {
            return _objExpenseMasterService.Queryable();
        }
        // GET: odata/ExpenseMasters(5)
        [EnableQuery]
        public SingleResult<ExpenseMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objExpenseMasterService.Queryable().Where(t => t.Id == key));
        }
        [HttpPost,ODataRoute("AlterExpenseMasterStatus")]
        public IHttpActionResult AlterExpenseMasterStatus(ODataActionParameters parameters)
        {
            object idsObj;
            List<long> ids=new List<long>();
            if (parameters.TryGetValue("ids", out idsObj))
            {
                var str = idsObj as string;
                if (!string.IsNullOrWhiteSpace(str))
                {
                    foreach (string s in str.Split(','))
                    {
                        try
                        {
                            ids.Add(long.Parse(s));
                        }
                        catch
                        {
                            return BadRequest($"Unable to Cast {s}");
                        }
                        
                    }
                }
            }
            if (ids.Count == 0)
            {
                return BadRequest("No Ids supplied");
            }
            _objExpenseMasterService.AlterStatus(ids);
            if (_unitOfWorkAsync.SaveChanges() > 0)
            {
                return Ok();
            }
            return NotFound();
        }
        // PUT: odata/ExpenseMasters(5)
        public async Task<IHttpActionResult> Put(long key, ExpenseMaster objExpenseMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objExpenseMaster.Id)
            {
                return BadRequest();
            }
            objExpenseMaster.ObjectState = ObjectState.Modified;
            _objExpenseMasterService.Update(objExpenseMaster);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objExpenseMaster);
        }
        // POST: odata/ExpenseMasters
        public async Task<IHttpActionResult> Post(ExpenseMaster objExpenseMaster)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            objExpenseMaster.ObjectState = ObjectState.Added;
            _objExpenseMasterService.Insert(objExpenseMaster);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ExpenseMasterExists(objExpenseMaster.Name))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objExpenseMaster);
        }
        //// PATCH: odata/ExpenseMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ExpenseMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ExpenseMaster objExpenseMaster = await _objExpenseMasterService.FindAsync(key);
            if (objExpenseMaster == null)
            {
                return NotFound();
            }
            objExpenseMaster.ObjectState = ObjectState.Modified;
            patch.Patch(objExpenseMaster);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ExpenseMasterExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objExpenseMaster);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objExpenseMaster = await _objExpenseMasterService.FindAsync(key);
            if (objExpenseMaster == null)
            {
                return NotFound();
            }
            objExpenseMaster.ObjectState = ObjectState.Deleted;
            _objExpenseMasterService.Delete(objExpenseMaster);
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

        private bool ExpenseMasterExists(string expenseName)
        {
            return _objExpenseMasterService.Query(e => e.Name == expenseName).Select().Any();
        }
        private bool ExpenseMasterExists(long key)
        {
            return _objExpenseMasterService.Query(e => e.Id == key).Select().Any();
        }
    }
}