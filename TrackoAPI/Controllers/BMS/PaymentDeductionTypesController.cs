using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.Repositories;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class PaymentDeductionTypesController : ODataController
        //ODataController
    {
        private readonly IRepositoryAsync<PaymentDeductionType> _repo;

        public PaymentDeductionTypesController(IRepositoryAsync<PaymentDeductionType> repo)
        {
            _repo = repo;
        }
        // GET: odata/PaymentDeductionTypes
        [HttpGet, EnableQuery]
        public IQueryable<PaymentDeductionType> Get()
        {
            return _repo.Queryable();
        }

        // GET: odata/PaymentDeductionTypes(5)
        [EnableQuery]
        public SingleResult<PaymentDeductionType> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/PaymentDeductionTypes(5)
        public async Task<IHttpActionResult> Put(long key, PaymentDeductionType objPaymentDeductionType)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objPaymentDeductionType.Id)
            {
                return BadRequest();
            }
            objPaymentDeductionType.ObjectState = ObjectState.Modified;
            _repo.Update(objPaymentDeductionType);

            try
            {
                await Request.GetContext().SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentDeductionTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objPaymentDeductionType);
        }
        // POST: odata/PaymentDeductionTypes
        public async Task<IHttpActionResult> Post(PaymentDeductionType objPaymentDeductionType)
        {
            objPaymentDeductionType.ObjectState = ObjectState.Added;
            _repo.Insert(objPaymentDeductionType);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (PaymentDeductionTypeExists(objPaymentDeductionType.TypeName, objPaymentDeductionType.Code))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Name or Code should be unique");
                }
                throw;
            }
            return Created(objPaymentDeductionType);
        }
        //// PATCH: odata/PaymentDeductionTypes(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PaymentDeductionType> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PaymentDeductionType objPaymentDeductionType = await _repo.FindAsync(key);
            if (objPaymentDeductionType == null)
            {
                return NotFound();
            }
            objPaymentDeductionType.ObjectState = ObjectState.Modified;
            patch.Patch(objPaymentDeductionType);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PaymentDeductionTypeExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objPaymentDeductionType);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objPaymentDeductionType = await _repo.FindAsync(key);
            if (objPaymentDeductionType == null)
            {
                return NotFound();
            }
            objPaymentDeductionType.ObjectState = ObjectState.Deleted;
            _repo.Delete(objPaymentDeductionType);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        public long GetLedgerId([FromODataUri]long key)
        {
            return _repo.Queryable().Where(x=>x.Id==key).Select(x => x.LedgerId).FirstOrDefault();
        }
        
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool PaymentDeductionTypeExists(string name, string code) => _repo.Query(e => (e.TypeName == name) || (e.Code == code)).Select().Any();
        private bool PaymentDeductionTypeExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}