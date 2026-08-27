using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Service.Finance;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ContactsController : ODataController
    //ODataController
    {
        private readonly IContactService _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public ContactsController(IUnitOfWorkAsync unitOfWorkAsync, IContactService service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/ContactBooks
        [HttpGet, EnableQuery]
        public IQueryable<Contact> Get() => _repo.Queryable();

        // GET: odata/ContactBooks(5)
        [EnableQuery]
        public SingleResult<Contact> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/ContactBooks(5)
        public async Task<IHttpActionResult> Put(long key, Contact objContact)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objContact.Id)
            {
                return BadRequest();
            }
            objContact.ObjectState = ObjectState.Modified;
            _repo.Update(objContact);

            try
            {
              //  await _unitOfWorkAsync.SaveChangesAsync();
                
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!ContactBookExists(key))
                //{
                //    return NotFound();
                //}
                //throw;
            }

            return Updated(objContact);
        }
        // POST: odata/ContactBooks
        public async Task<IHttpActionResult> Post(Contact objContact)
        {
            objContact.ObjectState = ObjectState.Added;
            _repo.Insert(objContact);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                //if (ContactBookExists(objContactBook.FirstName))
                //{
                //    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                //}
                //throw;
            }
            return Created(objContact);
        }
        //// PATCH: odata/ContactBooks(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<Contact> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Contact objContact = await _repo.FindAsync(key);
            if (objContact == null)
            {
                return NotFound();
            }
            objContact.ObjectState = ObjectState.Modified;
            patch.Patch(objContact);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!ContactBookExists(key))
                //{
                //    return NotFound();
                //}
                //throw;
            }

            return Updated(objContact);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objContactBook = await _repo.FindAsync(key);
            if (objContactBook == null)
            {
                return NotFound();
            }
            objContactBook.ObjectState = ObjectState.Deleted;
            _repo.Delete(objContactBook);
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

        //private bool ContactBookExists(string firstName) => _repo.Query(e => e.FirstName == firstName).Select().Any();
        //private bool ContactBookExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}