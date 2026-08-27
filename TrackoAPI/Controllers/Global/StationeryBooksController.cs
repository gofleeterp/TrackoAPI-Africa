using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class StationeryBooksController : ODataController
    {
        private readonly IStationeryBookService _objStationeryBookService;

        public StationeryBooksController(IStationeryBookService service)
        {
            _objStationeryBookService = service;
        }
        // GET: odata/Vouchers
        [HttpGet, EnableQuery]
        public IQueryable<StationeryBook> Get()
        {
            return _objStationeryBookService.Queryable();
        }
        // GET: odata/Vouchers(5)
        [EnableQuery]
        public SingleResult<StationeryBook> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objStationeryBookService.Queryable().Where(t => t.Id == key));
        }
        [HttpGet]
        public async Task<bool> IsBookUsed([FromODataUri] long key)
        {
            return
                await
                    Request.GetContext()
                        .RepositoryAsync<StationeryBookLogArchive>()
                        .Queryable()
                        .AnyAsync(x => x.BookId == key);
        } 
        // PUT: odata/Vouchers(5)
        public async Task<IHttpActionResult> Put(long key, StationeryBook objStationeryBook)
        {
            throw new BusinessException(ErrorCode.GLB107);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objStationeryBook.Id)
            {
                return BadRequest();
            }
            objStationeryBook.ObjectState = ObjectState.Modified;
            _objStationeryBookService.Update(objStationeryBook);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StationeryBookExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objStationeryBook);
        }
        // POST: odata/Vouchers
        public async Task<IHttpActionResult> Post(StationeryBook book)
        {
            var uow = Request.GetContext();
            book.ObjectState = ObjectState.Added;
            //objStationeryBook.OfficeId = null;
            //objStationeryBook.ClientId = null;
            
            try
            {
                if (book.AllotedDate != null)
                {
                    if (book.OfficeId == null && book.ClientId == null)
                    {
                        return BadRequest("Either Office or Client is Required to Complete Stationary Allotment");
                    }
                    _objStationeryBookService.MapBook(book, uow);
                }
                else
                {
                    _objStationeryBookService.Insert(book);
                    await Request.GetContext().SaveChangesAsync();
                }
                
            }
            catch (DbUpdateException)
            {
                if (StationeryBookExists(book.Name))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }
            return Created(book);
        }
        [HttpPost,ODataRoute("UpdateBookMapping")]
        public async Task<IHttpActionResult> UpdateBookMapping(ODataActionParameters param)
        {
            var models = param["books"] as IEnumerator<vwStationaryMapping>;
            
            if (models == null)
                return BadRequest("Missing Books data");

            var unmapped = string.Empty;
            foreach (var model in models.ToList())
            {
                if (model == null || model.BookId <= 0) return BadRequest("Missing Book data");
                //if (model.OfficeId.GetValueOrDefault() <= 0 && (model.IssueDate.HasValue && model.IssueDate != default(DateTime))) return BadRequest("Office is required");
                if ((!model.IssueDate.HasValue || model.IssueDate == default(DateTime)) && model.OfficeId.GetValueOrDefault() > 0) return BadRequest("Issue Date is required");
                var book = await _objStationeryBookService.FindAsync(model.BookId);
                if (book == null)
                {
                    return NotFound();
                }
                
                if (!book.AllotedDate.HasValue && model.IssueDate.HasValue)
                {
                    if (book.ExpiryDate.HasValue && book.ExpiryDate.Value <= model.IssueDate.Value)
                    {
                        unmapped += (unmapped.Length > 0 ? "," : "") + book.Name + "=>With Error: Allotted Date cannot be greater than Expiry Date";
                        continue;
                    }
                        try
                    {
                        //TODO:Create a Book Log, as the book has been alloted
                        book.AllotedDate = model.IssueDate;
                        book.ClientId = model.ClientId;
                        book.IssueToPerson = model.IssuedTo;
                        book.MappingRemark = model.MappingRemark;
                        book.OfficeId = model.OfficeId;
                        book.ObjectState = ObjectState.Modified;
                        _objStationeryBookService.MapBook(book, Request.GetContext());
                       // return Ok();
                    }
                    catch (Exception ex)
                    {
                        unmapped += (unmapped.Length > 0 ? "," : "") + book.Name + "=>With Error:" +
                                    ex.GetBaseException().Message;
                    }
                }
                else
                {
                    unmapped += (unmapped.Length > 0 ? "," : "") + book.Name + "=>With Error: Allotted Date Condition Failed";
                }
                
            }
            if (!string.IsNullOrWhiteSpace(unmapped)) return BadRequest("Unable to Map few books i.e." + unmapped);
            return Ok();
        }
        //// PATCH: odata/Vouchers(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<StationeryBook> patch)
        {
            var uow = Request.GetContext();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            StationeryBook book = await _objStationeryBookService.FindAsync(key);
            if (book == null)
            {
                return NotFound();
            }
            
            var updated=patch.GetEntity();
            if (book.AllotedDate.HasValue && !updated.AllotedDate.HasValue)
            {
                if (book.IsLocked || book.IsUsed) return BadRequest("Book has been used so cannot unallocate book");
                //TODO:Remove Book Log as Allotment has been revoked
                _objStationeryBookService.RevokeBookIssue(book, uow);
                return Updated(book);
            }
            if (!book.AllotedDate.HasValue && updated.AllotedDate.HasValue)
            {
                //TODO:Create a Book Log, as the book has been alloted
                patch.Patch(book);
                book.ObjectState = ObjectState.Modified;
                _objStationeryBookService.MapBook(book, uow);
                return Updated(book);

            }
            if ((book.AllotedDate.HasValue && updated.AllotedDate.HasValue && book.AllotedDate != updated.AllotedDate)||book.OfficeId!=updated.OfficeId|| book.ClientId != updated.ClientId)
            {
                if (book.IsLocked || book.IsUsed) return BadRequest("Book has been used so cannot update book");
                //TODO:Change Alloted date in BookLog
                book.AllotedDate = updated.AllotedDate;
                book.ClientId = updated.ClientId;
                book.IssueToPerson = updated.IssueToPerson;
                book.MappingRemark = updated.MappingRemark;
                book.OfficeId = updated.OfficeId;
                book.ObjectState = ObjectState.Modified;
                _objStationeryBookService.UpdateBookIssueDate(book, uow);
                return Updated(book);
            }
            if(book.ExpiryDate!=updated.ExpiryDate)
            {
                uow.BeginTransaction();
                try
                {
                    _objStationeryBookService.UpdateExpiryDate(book.Id, updated.ExpiryDate, uow);
                    book.ExpiryDate = updated.ExpiryDate;
                    book.ObjectState = ObjectState.Modified;
                    await Request.GetContext().SaveChangesAsync();
                    uow.Commit();
                    return Updated(book);
                }
                catch (Exception)
                {
                    uow.Rollback();

                    throw;
                }
                
            }

            patch.Patch(book);
            book.ObjectState = ObjectState.Modified;
            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StationeryBookExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(book);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objStationeryBook = await _objStationeryBookService.FindAsync(key);
            if (objStationeryBook == null)
            {
                return NotFound();
            }
            if (objStationeryBook.IsLocked || objStationeryBook.IsUsed) return BadRequest("Book has been used so cannot delete book");
            objStationeryBook.ObjectState = ObjectState.Deleted;
            _objStationeryBookService.Delete(objStationeryBook);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Request.GetContext().Dispose();
            }
            base.Dispose(disposing);
        }

        private bool StationeryBookExists(string name)
        {
            return _objStationeryBookService.Query(e => e.Name == name).Select().Any();
        }
        private bool StationeryBookExists(long key)
        {
            return _objStationeryBookService.Query(e => e.Id == key).Select().Any();
        }
    }
}