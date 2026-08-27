using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;

using Repository.Pattern.Core.Repositories;
using Repository.Pattern.Core.UnitOfWork;

using RestSharp;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service.Finance;
using TrackoApi.Service.Global;

using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class HttpRequestPoolsController : ODataController
    //ODataController
    {
        private readonly IRepositoryAsync<HttpRequestPool> _repo;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public HttpRequestPoolsController(IUnitOfWorkAsync unitOfWorkAsync, IRepositoryAsync<HttpRequestPool> service)
        {
            _repo = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/HttpRequestPoolBooks
        [HttpGet, EnableQuery]
        public IQueryable<HttpRequestPool> Get() => _repo.Queryable();

        // GET: odata/HttpRequestPoolBooks(5)
        [EnableQuery]
        public SingleResult<HttpRequestPool> Get([FromODataUri] string key) => SingleResult.Create(_repo.Queryable().Where(t => t.RequestId == key));
        // PUT: odata/HttpRequestPoolBooks(5)
        public async Task<IHttpActionResult> Put(string key, HttpRequestPool entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != entity.RequestId)
            {
                return BadRequest();
            }
            _repo.Update(entity);

            try
            {
              //  await _unitOfWorkAsync.SaveChangesAsync();
                
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!HttpRequestPoolBookExists(key))
                //{
                //    return NotFound();
                //}
                throw;
            }

            return Updated(entity);
        }
        // POST: odata/HttpRequestPoolBooks
        public async Task<IHttpActionResult> Post(HttpRequestPool entity)
        {
            _repo.Insert(entity);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                //if (HttpRequestPoolBookExists(entityBook.FirstName))
                //{
                //    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                //}
                throw;
            }
            return Created(entity);
        }
        //// PATCH: odata/HttpRequestPoolBooks(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] string key, Delta<HttpRequestPool> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            HttpRequestPool entity = await _repo.FindAsync(key);
            if (entity == null)
            {
                return NotFound();
            }
            patch.Patch(entity);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                //if (!HttpRequestPoolBookExists(key))
                //{
                //    return NotFound();
                //}
                throw;
            }

            return Updated(entity);
        }

        public async Task<IHttpActionResult> BulkPost(ODataActionParameters parmater)
        {
            var entities=parmater["entities"] as IEnumerator<HttpRequestPool>;
            if (entities==null)
            {
                return BadRequest("Entities Parameter was null");
            }
            if (!(parmater["getresponse"] is bool getresponse))
            {
                return BadRequest("GetResponse Parameter was null");
            }
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var ets = entities.ToList();
            if (!getresponse)
            {
                _repo.InsertRange(ets);
                try
                {
                    await _unitOfWorkAsync.SaveChangesAsync();
                }
                catch (DbUpdateException)
                {
                    throw;
                }
            }
            else
            {
                var tasks = new List<Task>();
                RestClient client = new RestClient();
                foreach (var req in ets)
                {
                    tasks.Add(client.AddRequest(req));
                }
                await Task.WhenAll(tasks);
                return Json(ets.Select(x => new {
                x.RequestId,
                x.Result,
                LogData = x.LogRequest?x.LogData:string.Empty
                }).ToList(),new Newtonsoft.Json.JsonSerializerSettings());

            }
            return Ok("OK");
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(string key)
        {
            var entityBook = await _repo.FindAsync(key);
            if (entityBook == null)
            {
                return NotFound();
            }
            _repo.Delete(entityBook);
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

        //private bool HttpRequestPoolBookExists(string firstName) => _repo.Query(e => e.FirstName == firstName).Select().Any();
        //private bool HttpRequestPoolBookExists(string key) => _repo.Query(e => e.Id == key).Select().Any();
    }
}