using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using System.Web.OData.Routing;
using TrackoApi.Models.Global;
using System;
using System.Runtime.Remoting;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CurrencyConversionsController : ODataController
    {
        private readonly ICurrencyConversionService _objCurrencyConversionService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;
        public CurrencyConversionsController(IUnitOfWorkAsync unitOfWorkAsync, ICurrencyConversionService service)
        {
            _objCurrencyConversionService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/CurrencyConversions
        [HttpGet, EnableQuery]
        public IQueryable<CurrencyConversion> Get()
        {
            return _objCurrencyConversionService.Queryable();
        }
        // GET: odata/CurrencyConversions(5)
        [EnableQuery]
        public SingleResult<CurrencyConversion> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objCurrencyConversionService.Queryable().Where(t => t.Id == key));
        }
        private bool CCExists(long key,long CurTypeId,DateTime CurDate)
        {
            return _objCurrencyConversionService.Query(e => e.CurTypeId == CurTypeId && e.CurDate==CurDate && e.Id!=key).Select().Any();
        }
        // PUT: odata/CurrencyConversions(5)
        public async Task<IHttpActionResult> Put([FromODataUri] long key, CurrencyConversion obj)
        {   
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != obj.Id)
            {
                return BadRequest();
            }

            if (CCExists(key, obj.CurTypeId, obj.CurDate.Value))
            {
                return BadRequest("Record already exists.");
            }


            obj.ObjectState = ObjectState.Modified;
            _objCurrencyConversionService.Update(obj);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(obj);
        }
        // POST: odata/CurrencyConversions
        public async Task<IHttpActionResult> Post(CurrencyConversion obj)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            
            if (CCExists(0, obj.CurTypeId, obj.CurDate.Value))
            {
                return BadRequest("Record already exists.");
            }

            obj.ObjectState = ObjectState.Added;
            _objCurrencyConversionService.Insert(obj);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateException)
            {
                throw;
            }
            return Created(obj);
        }
        //// PATCH: odata/CurrencyConversions(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CurrencyConversion> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            CurrencyConversion obj = await _objCurrencyConversionService.FindAsync(key);
            if (obj == null)
            {
                return NotFound();
            }
            obj.ObjectState = ObjectState.Modified;

            patch.Patch(obj);

            if (CCExists(obj.Id, obj.CurTypeId, obj.CurDate.Value))
            {
                return BadRequest("Record already exists.");
            }

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();

            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(obj);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            ////var objCurrencyConversion = await _objCurrencyConversionService.FindAsync(key);
            ////if (objCurrencyConversion == null)
            ////{
            ////    return NotFound();
            ////}
            ////objCurrencyConversion.ObjectState = ObjectState.Modified;
            ////_objCurrencyConversionService.Delete(objCurrencyConversion);
            ////await _unitOfWorkAsync.SaveChangesAsync();
            ////return StatusCode(HttpStatusCode.NoContent);
            ///

            /*Deletion is not allowed*/

            var obj = await _objCurrencyConversionService.FindAsync(key);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != obj.Id)
            {
                return BadRequest();
            }
            obj.IsActive = false;
            obj.ObjectState = ObjectState.Modified;
            _objCurrencyConversionService.Update(obj);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                throw;
            }

            return Updated(obj);
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