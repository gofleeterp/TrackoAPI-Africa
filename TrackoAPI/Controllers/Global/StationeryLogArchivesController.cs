using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.ModelBinding;
using Repository.Pattern.Core;
using Repository.Pattern.Core.UnitOfWork;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class StationeryLogArchivesController : ODataController
    //ODataController
    {
        private readonly IStationeryBookLogArchiveService _objStationeryBookLogArchiveService;
        private readonly IUnitOfWorkAsync _unitOfWorkAsync;

        public StationeryLogArchivesController(IUnitOfWorkAsync unitOfWorkAsync, IStationeryBookLogArchiveService service)
        {
            _objStationeryBookLogArchiveService = service;
            _unitOfWorkAsync = unitOfWorkAsync;
        }
        // GET: odata/Vouchers
        [HttpGet, EnableQuery]
        public IQueryable<StationeryBookLogArchive> Get()
        {
            return _objStationeryBookLogArchiveService.Queryable();
        }
        // GET: odata/Vouchers(5)
        [EnableQuery]
        public SingleResult<StationeryBookLogArchive> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objStationeryBookLogArchiveService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/Vouchers(5)
        public async Task<IHttpActionResult> Put(long key, StationeryBookLogArchive objStationeryBookLogArchive)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objStationeryBookLogArchive.Id)
            {
                return BadRequest();
            }
            objStationeryBookLogArchive.ObjectState = ObjectState.Modified;
            _objStationeryBookLogArchiveService.Update(objStationeryBookLogArchive);

            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StationeryBookLogArchiveExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objStationeryBookLogArchive);
        }
        // POST: odata/Vouchers
        public async Task<IHttpActionResult> Post(StationeryBookLogArchive objStationeryBookLogArchive)
        {
            objStationeryBookLogArchive.ObjectState = ObjectState.Added;
            _objStationeryBookLogArchiveService.Insert(objStationeryBookLogArchive);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (StationeryBookLogArchiveExists(objStationeryBookLogArchive.BookId,objStationeryBookLogArchive.PageNo))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(objStationeryBookLogArchive);
        }
        //// PATCH: odata/Vouchers(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<StationeryBookLogArchive> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StationeryBookLogArchive objStationeryBookLogArchive = await _objStationeryBookLogArchiveService.FindAsync(key);
            if (objStationeryBookLogArchive == null)
            {
                return NotFound();
            }
            objStationeryBookLogArchive.ObjectState = ObjectState.Modified;
            patch.Patch(objStationeryBookLogArchive);
            try
            {
                await _unitOfWorkAsync.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StationeryBookLogArchiveExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objStationeryBookLogArchive);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objStationeryBookLogArchive = await _objStationeryBookLogArchiveService.FindAsync(key);
            if (objStationeryBookLogArchive == null)
            {
                return NotFound();
            }
            objStationeryBookLogArchive.ObjectState = ObjectState.Deleted;
            _objStationeryBookLogArchiveService.Delete(objStationeryBookLogArchive);
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

        private bool StationeryBookLogArchiveExists(long bookId,string pageNo)
        {
            return _objStationeryBookLogArchiveService.Query(e => e.BookId == bookId && e.PageNo==pageNo).Select().Any();
        }
        private bool StationeryBookLogArchiveExists(long key)
        {
            return _objStationeryBookLogArchiveService.Query(e => e.Id == key).Select().Any();
        }
    }
}