using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using Service.Pattern;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global.CronJobs;
using TrackoApi.Service.Global;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers.Global
{
    [AuthorizeEx]
    public class TemplateMastersController : ODataController
    //ODataController
    {
        private readonly IService<TemplateMaster> _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public TemplateMastersController(IUnitOfWorkAsync unitOfWorkAsync, IService<TemplateMaster> service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/TemplateMaster
        [HttpGet, EnableQuery]
        public IQueryable<TemplateMaster> Get() => _repo.Queryable();

        // GET: odata/TemplateMasters(5)
        [EnableQuery]
        public SingleResult<TemplateMaster> Get([FromODataUri] long key) => SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        // PUT: odata/TemplateMasters(5)
        public async Task<IHttpActionResult> Put(long key, TemplateMaster entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.Id)
            {
                return BadRequest();
            }
            entity.ObjectState = ObjectState.Modified;
            _repo.Update(entity);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TemplateExists(key))
                {
                    return NotFound();
                }
                if (TemplateExists(entity))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Template has already configured..");
                }
                throw;
            }

            return Updated(entity);
        }

        

        // POST: odata/TemplateMasters
        public async Task<IHttpActionResult> Post(TemplateMaster entity)
        {
            entity.ObjectState = ObjectState.Added;
            _repo.Insert(entity);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (TemplateExists(entity))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Template has already configured..");
                }
                throw;
            }
            return Created(entity);
        }

        

        //// PATCH: odata/TemplateMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<TemplateMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            TemplateMaster entity = await _repo.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Modified;
            patch.Patch(entity);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TemplateExists(key))
                {
                    return NotFound();
                }
                if (TemplateExists(entity))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Template has already configured..");
                }
                throw;
            }

            return Updated(entity);
        }
        // DELETE: odata/TemplateMasters(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var entity = await _repo.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            entity.ObjectState = ObjectState.Deleted;
            _repo.Delete(entity);
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
        private bool TemplateExists(long key)
        {
            return _repo.Query(e => e.Id == key).Select().Any();
        }
        private bool TemplateExists(TemplateMaster entity)
        {
            return _repo.Query(x => x.EntityType == entity.EntityType && x.EventType == entity.EventType && x.Ref1Id == entity.Ref1Id && x.Ref2Id == entity.Ref2Id && x.Ref3Id == x.Ref3Id && x.MessageType == entity.MessageType && x.Id != entity.Id).Select().Any();
        }
        //private bool ContactBookExists(string firstName) => _repo.Query(e => e.FirstName == firstName).Select().Any();
        //private bool ContactBookExists(long key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}