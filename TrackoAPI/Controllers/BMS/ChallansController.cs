using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using AutoMapper;
using Newtonsoft.Json;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.BMS;
using TrackoApi.Models.FMS;
using TrackoApi.Service.TMS;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ChallansController : ODataController
    //ODataController
    {
        private readonly IChallanService _objChallanService;

        public ChallansController(IChallanService service)
        {
            _objChallanService = service;
        }
        // GET: odata/ChallanMasters
        [HttpGet, EnableQuery(MaxExpansionDepth = 5)]
        public IQueryable<ChallanMaster> Get()
        {
            return _objChallanService.Queryable();
        }
        // GET: odata/ChallanMasters(5)
        [EnableQuery(MaxExpansionDepth = 5)]
        public SingleResult<ChallanMaster> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objChallanService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/ChallanMasters(5)
        public async Task<IHttpActionResult> Put(long key, ChallanMaster challan)
        {
            var uow= Request.GetContext();
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != challan.Id)
            {
                return BadRequest();
            }
            challan.ObjectState = ObjectState.Modified;
            _objChallanService.Update(challan);
            if (!string.IsNullOrWhiteSpace(challan.CnChallanJson))
            {
                try
                {
                    var cnchallans = JsonConvert.DeserializeObject<List<CnChallan>>(challan.CnChallanJson);
                    if (cnchallans.Any())
                    {

                        var repo = uow.RepositoryAsync<CnChallan>();
                        foreach (var chcn in cnchallans)
                        {
                            if (chcn.Id > 0)
                            {
                                var chcnorg = await repo.FindAsync(chcn.Id);
                                Mapper.Map(chcn, chcnorg);
                                chcnorg.TriplogId = challan.TriplogId;
                                chcnorg.fk_Triplog = challan.fk_Triplog;
                                chcnorg.ChallanId = challan.Id;
                                chcnorg.fk_Challan = challan;
                                chcnorg.ObjectState = ObjectState.Modified;
                            }
                            else
                            {
                                chcn.TriplogId = challan.TriplogId;
                                chcn.fk_Triplog = challan.fk_Triplog;
                                chcn.ChallanId = challan.Id;
                                chcn.fk_Challan = challan;
                                chcn.ObjectState = ObjectState.Added;
                                repo.Insert(chcn);
                            }


                        }
                    }

                }
                catch
                {
                    return BadRequest("Malformed data found in CnChallan section");
                }
            }
            await uow.SaveChangesAsync();

            return Updated(challan);
        }
        // POST: odata/ChallanMasters
        public async Task<IHttpActionResult> Post(ChallanMaster challan)
        {
            challan.ObjectState = ObjectState.Added;
            var uow = Request.GetContext();
            await _objChallanService.InsertAsync(challan).ConfigureAwait(true);
            if (!string.IsNullOrWhiteSpace(challan.CnChallanJson))
            {
                try
                {
                    var cnchallans = JsonConvert.DeserializeObject<List<CnChallan>>(challan.CnChallanJson);
                    if (cnchallans.Any())
                    {

                        var repo = uow.RepositoryAsync<CnChallan>();
                        foreach (var chcn in cnchallans)
                        {
                            if (chcn.Id > 0)
                            {
                                var chcnorg = await repo.FindAsync(chcn.Id).ConfigureAwait(true);
                                Mapper.Map(chcn, chcnorg);
                                chcnorg.TriplogId = challan.TriplogId;
                                chcnorg.fk_Triplog = challan.fk_Triplog;
                                chcnorg.ChallanId = challan.Id;
                                chcnorg.fk_Challan = challan;
                                chcnorg.ObjectState = ObjectState.Modified;
                            }
                            else
                            {
                                chcn.TriplogId = challan.TriplogId;
                                chcn.fk_Triplog = challan.fk_Triplog;
                                chcn.ChallanId = challan.Id;
                                chcn.fk_Challan = challan;
                                chcn.ObjectState = ObjectState.Added;
                                repo.Insert(chcn);
                            }


                        }
                    }

                }
                catch
                {
                    return BadRequest("Malformed data found in CnChallan section");
                }
            }
            await uow.SaveChangesAsync().ConfigureAwait(true);
            return Created(challan);
        }
        // POST: odata/VehicleMovementLogs(key)/Challans
        [ODataRoute("Challans({key})/CNChallans")]
        public async Task<IHttpActionResult> PostCNChallans([FromODataUri]long key, [FromBody] CnChallan cnchallan)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var uow = Request.GetContext();
            var ch = await uow.RepositoryAsync<ChallanMaster>().FindAsync(key);
            cnchallan.ChallanId = ch.Id;
            cnchallan.ObjectState = ObjectState.Added;
            cnchallan.TriplogId = ch.TriplogId;
            var item = uow.RepositoryAsync<CnChallan>().Insert(cnchallan);
            await uow.SaveChangesAsync();
            return Created(item);
        }
       
        //// PATCH: odata/ChallanMasters(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ChallanMaster> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ChallanMaster challan = await _objChallanService.FindAsync(key);
            if (challan == null)
            {
                return NotFound();
            }
            challan.ObjectState = ObjectState.Modified;
            patch.Patch(challan);
            var uow = Request.GetContext();
            if (!string.IsNullOrWhiteSpace(challan.CnChallanJson))
            {
                try
                {
                    var cnchallans = JsonConvert.DeserializeObject<List<CnChallan>>(challan.CnChallanJson);
                    if (cnchallans.Any())
                    {

                        var repo = uow.RepositoryAsync<CnChallan>();
                        foreach (var chcn in cnchallans)
                        {
                            if (chcn.Id > 0)
                            {
                                var chcnorg = await repo.FindAsync(chcn.Id).ConfigureAwait(true);
                                Mapper.Map(chcn, chcnorg);
                                chcnorg.TriplogId = challan.TriplogId;
                                chcnorg.fk_Triplog = challan.fk_Triplog;
                                chcnorg.ChallanId = challan.Id;
                                chcnorg.fk_Challan = challan;
                                chcnorg.ObjectState = ObjectState.Modified;
                            }
                            else
                            {
                                chcn.TriplogId = challan.TriplogId;
                                chcn.fk_Triplog = challan.fk_Triplog;
                                chcn.ChallanId = challan.Id;
                                chcn.fk_Challan = challan;
                                chcn.ObjectState = ObjectState.Added;
                                repo.Insert(chcn);
                            }


                        }
                    }

                }
                catch
                {
                    return BadRequest("Malformed data found in CnChallan section");
                }
            }
            if (challan.TriplogId.HasValue)
            {
                var tl = uow.RepositoryAsync<VehicleMovementLog>().Queryable().Include(x => x.Challans).FirstOrDefault(x => x.Id == challan.TriplogId.Value);
                if (tl == null)
                {
                    return NotFound();
                }
                tl.LoadingQty = tl.Challans.Sum(x => x.Quantity);
                tl.ObjectState=ObjectState.Modified;
            }
            await uow.SaveChangesAsync();

            return Updated(challan);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var uow = Request.GetContext();
            if (!Request.IsBatchRequest())
            {
                uow.BeginTransaction();
            }

            try
            {
                var objChallanMaster = await _objChallanService.FindAsync(key);
                if (objChallanMaster == null)
                {
                    return NotFound();
                }
                //await uow.RepositoryAsync<CNStockMMLog>().Queryable().Where(x => x.fk_ChallanCN.ChallanId == key).DeleteAsync();
                //await uow.RepositoryAsync<CNStockLog>().Queryable().Where(x => x.fk_ChallanCN.ChallanId == key).DeleteAsync();
                //await uow.RepositoryAsync<CnChallan>().Queryable().Where(x => x.ChallanId == key).DeleteAsync();
                var cnchalns = uow.RepositoryAsync<CnChallan>().Queryable().Where(x => x.ChallanId == objChallanMaster.Id).ToList();
                if (cnchalns != null)
                {
                    cnchalns.ForEach(x => x.ObjectState = ObjectState.Deleted);
                }
                await Request.GetContext().SaveChangesAsync();
                objChallanMaster.ObjectState = ObjectState.Deleted;
                _objChallanService.Delete(objChallanMaster);
                await Request.GetContext().SaveChangesAsync();
                if (!Request.IsBatchRequest())
                {
                    uow.Commit();
                }
                return StatusCode(HttpStatusCode.NoContent);
            }
            catch
            {
                uow.Rollback();
                throw;
            }


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
        public async Task<IHttpActionResult> CreateRef([FromODataUri] int key, string navigationProperty, [FromBody] Uri link)
        {
            var cnch = await _objChallanService.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (cnch == null)
            {
                return NotFound();
            }
            var uow = Request.GetContext();
            var id = Request.GetKeyFromUri<long>(link);

            var triplogid = uow.RepositoryAsync<VehicleMovementLog>();
            var tripid =
                await
                    triplogid.Queryable().AnyAsync(x => x.Id == id);
            if (!tripid)
            {
                return NotFound();
            }
            cnch.TriplogId = id;
            cnch.ObjectState = ObjectState.Modified;
            await uow.SaveChangesAsync();

            //switch (navigationProperty)
            //{
            //    case "fk_TripLog":               

            //        break;               
            //    default:
            //        return StatusCode(HttpStatusCode.NotImplemented);
            //}
            await uow.SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

    }
}