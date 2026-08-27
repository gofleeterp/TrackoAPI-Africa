using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using Newtonsoft.Json;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service.TMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class CNExtraInfosController : ODataController
    //ODataController
    {
        private readonly ICNExtraInfoService _repo;

        public CNExtraInfosController(ICNExtraInfoService service)
        {
            _repo = service;
        }
        // GET: odata/CNExtraInfos
        [HttpGet, EnableQuery]
        public IQueryable<CNExtraInfo> Get()
        {
            return _repo.Queryable();
        }
        // GET: odata/CNExtraInfos(5)
        [EnableQuery]
        public SingleResult<CNExtraInfo> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_repo.Queryable().Where(t => t.Id == key));
        }

        // PUT: odata/CNExtraInfos(5)
        public async Task<IHttpActionResult> Put(long key, CNExtraInfo objCNExtraInfo)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objCNExtraInfo.Id)
            {
                return BadRequest();
            }
            objCNExtraInfo.ObjectState = ObjectState.Modified;
            _repo.Update(objCNExtraInfo);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objCNExtraInfo);
        }

        // POST: odata/CNExtraInfos
        public async Task<IHttpActionResult> Post(CNExtraInfo objCNExtraInfo)
        {
            var dt = objCNExtraInfo.Data ?? new List<JsonDataEntity>();
            if (dt.Any())
            {
                objCNExtraInfo.DataProps = JsonConvert.SerializeObject(dt);
            }
            objCNExtraInfo.ObjectState = ObjectState.Added;
            var cnextra= _repo.Insert(objCNExtraInfo);
            await Request.GetContext().SaveChangesAsync();
            return Created(cnextra);
        }

        //// PATCH: odata/CNExtraInfos(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<CNExtraInfo> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            CNExtraInfo cnextra = await _repo.FindAsync(key);
            if (cnextra == null)
            {
                return NotFound();
            }
            var dataview = patch.GetEntity().Data;
            patch.Patch(cnextra);
            if (dataview != null && dataview.Any())
            {
                foreach (var je in dataview)
                {
                    cnextra.DeleteAndAdd(je);
                }
            }
            cnextra.ObjectState = ObjectState.Modified;
            await Request.GetContext().SaveChangesAsync();
            return Updated(cnextra);
        }

        // DELETE: odata/CNExtraInfos(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objCNExtraInfo = await _repo.FindAsync(key);
            if (objCNExtraInfo == null)
            {
                return NotFound();
            }
            objCNExtraInfo.ObjectState = ObjectState.Deleted;
            _repo.Delete(objCNExtraInfo);
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
        [AcceptVerbs("POST", "PUT")]
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key,
        string navigationProperty, [FromBody] Uri link)
        {
            var pod = await _repo.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (pod == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);
            switch (navigationProperty)
            {
                case "fk_DSVoucher":
                    pod.DSVoucherId = id;
                    pod.ObjectState = ObjectState.Modified;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
        [AcceptVerbs("DELETE")]
        public async Task<IHttpActionResult> DeleteRef([FromODataUri] long key,
        string navigationProperty, [FromBody] Uri link)
        {
            var uow = Request.GetContext();
            var pod = await _repo.FindAsync(key);
            if (pod == null)
            {
                return NotFound();
            }

            switch (navigationProperty)
            {
                case "fk_DSVoucher":
                    pod.fk_DSVoucher = null;
                    pod.DSVoucherId = null;
                    pod.ObjectState = ObjectState.Modified;
                    break;
                default:
                    return StatusCode(HttpStatusCode.NotImplemented);
            }
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }
    }
}