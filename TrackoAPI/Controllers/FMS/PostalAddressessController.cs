using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
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
    public class PostalAddresseesController : ODataController
    //ODataController
    {
        private readonly IPostalAddressService _objPostalAddressService;
        //private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public PostalAddresseesController(IPostalAddressService service)
        {
            _objPostalAddressService = service;
           // _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/PostalAddresss
        [HttpGet, EnableQuery]
        public IQueryable<PostalAddress> Get()
        {
            return _objPostalAddressService.Queryable();
        }
        // GET: odata/PostalAddresss(5)
        [EnableQuery]
        public SingleResult<PostalAddress> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objPostalAddressService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/PostalAddresss(5)
        public async Task<IHttpActionResult> Put(long key, PostalAddress objPostalAddress)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objPostalAddress.Id)
            {
                return BadRequest();
            }
            objPostalAddress.ObjectState = ObjectState.Modified;
            _objPostalAddressService.Update(objPostalAddress);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(objPostalAddress);
        }
        // POST: odata/PostalAddresss
        public async Task<IHttpActionResult> Post(PostalAddress objPostalAddress)
        {
            objPostalAddress.ObjectState = ObjectState.Added;
            _objPostalAddressService.Insert(objPostalAddress);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(objPostalAddress);
        }
        //// PATCH: odata/PostalAddresss(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<PostalAddress> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            PostalAddress objPostalAddress = await _objPostalAddressService.FindAsync(key);
            if (objPostalAddress == null)
            {
                return NotFound();
            }
            objPostalAddress.ObjectState = ObjectState.Modified;
            patch.Patch(objPostalAddress);
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                
                throw;
            }

            return Updated(objPostalAddress);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objPostalAddress = await _objPostalAddressService.FindAsync(key);
            if (objPostalAddress == null)
            {
                return NotFound();
            }
            objPostalAddress.ObjectState = ObjectState.Deleted;
            _objPostalAddressService.Delete(objPostalAddress);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                //Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }
    }
}