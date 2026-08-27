using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Models.Shared;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ObjectClassesController:ODataController
    {
        private readonly IObjectClassService _objectClassService;
        public ObjectClassesController(IObjectClassService service)
        {
            _objectClassService = service;
        }
        // GET: odata/ObjectClasses
        [HttpGet,EnableQuery]
        public IQueryable<ObjectClass> Get()
        {
            return _objectClassService.Queryable();
        }
        // GET: odata/ObjectClass(5)
        [EnableQuery]
        public SingleResult<ObjectClass> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objectClassService.Queryable().Where(t => t.Id == key));
        }
        // GET: odata/ObjectClasses(5)/Category
        [EnableQuery]
        public SingleResult<ObjectCategory> GetCategory([FromODataUri] long key)
        {
            return SingleResult.Create(_objectClassService.Queryable().Where(t => t.Id == key).Select(x=>x.Category));
        }
        [HttpGet,ODataRoute("ObjectClasses({Key})/ObjectMappings")]
        public IQueryable<ObjectClassMap> GetObjectMappings([FromODataUri] long key)
        {
            var uow = Request.GetContext();
           // var cls = _objectClassService.Queryable().Include(x => x.Category).Include(x=>x.ObjectMappings).Where(x => x.Id == key).SelectMany();
            var cls =
                uow.Repository<ObjectClassMap>().Queryable().Include(x => x.fk_Category).Include(x=>x.fk_Class).Where(x => x.ClassId == key);
            var roleTypeId = cls.Select(x => x.fk_Category.RoleTypeId).FirstOrDefault();
            switch (roleTypeId)
            {
                case 1145:
                    var ac1145 = (from a in uow.Repository<LedgerRole>().Queryable()
                        join b in cls on a.fk_Ledger.AccountRoleId equals b.fk_Category.RoleId
                        where !a.fk_Ledger.IsDefaulter && b.ObjectId == a.LedgerId
                                  let c = new
                        {
                            Id = b.Id,
                            ObjectId = a.LedgerId,
                            ClassId = b.ClassId,
                            CategoryId = b.CategoryId,
                            ObjectName = a.fk_Ledger.AccountName
                        }
                        select c).ToList();
                    return ac1145.Select(x=>new ObjectClassMap()
                    {
                        Id = x.Id,
                        CategoryId = x.CategoryId,
                        ClassId = x.ClassId,
                        ObjectId = x.ObjectId,
                        ObjectName = x.ObjectName
                    }).AsQueryable();
                case 1146:
                    var ac1146 = (from a in uow.Repository<Ledger>().Queryable()
                             join b in cls on a.GroupId equals b.fk_Category.RoleId
                             where !a.IsDefaulter && b.ObjectId==a.Id
                                 let c = new
                                 {
                                     Id = b.Id,
                                     ObjectId = a.Id,
                                     ClassId = b.ClassId,
                                     CategoryId = b.CategoryId,
                                     ObjectName = a.AccountName
                                 }
                                 select c).ToList();
                    return ac1146.Select(x => new ObjectClassMap()
                    {
                        Id = x.Id,
                        CategoryId = x.CategoryId,
                        ClassId = x.ClassId,
                        ObjectId = x.ObjectId,
                        ObjectName = x.ObjectName
                    }).AsQueryable();
                case 1292:
                    var offices = (from a in uow.Repository<OfficeMaster>().Queryable()
                                  join b in cls on 1292 equals b.fk_Category.RoleId
                                  where a.Status == MasterStatus.Active && b.ObjectId == a.Id
                                  let c = new
                                  {
                                      Id = b.Id,
                                      ObjectId = a.Id,
                                      ClassId = b.ClassId,
                                      CategoryId = b.CategoryId,
                                      ObjectName = a.OfficeName
                                  }
                                  select c).ToList();
                    return offices.Select(x => new ObjectClassMap()
                    {
                        Id = x.Id,
                        CategoryId = x.CategoryId,
                        ClassId = x.ClassId,
                        ObjectId = x.ObjectId,
                        ObjectName = x.ObjectName
                    }).AsQueryable();
                default:
                    return new List<ObjectClassMap>().AsQueryable();
            }
        }

        

        //PUT: odata/ObjectClassses(5)
        public async Task<IHttpActionResult> Put(long key, ObjectClass objectClass)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objectClass.Id)
            {
                return BadRequest();
            }
            objectClass.ObjectState=ObjectState.Modified;
            _objectClassService.Update(objectClass);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObjectClassExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objectClass);
        }
        // POST: odata/ObjectClasss
        public async Task<IHttpActionResult> Post(ObjectClass objectClass)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            objectClass.ObjectState = ObjectState.Added;
            _objectClassService.Insert(objectClass);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ObjectClassExists(objectClass.ClassName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }

            return Created(objectClass);
        }
        //// PATCH: odata/ObjectClassses(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ObjectClass> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectClass objectClass = await _objectClassService.FindAsync(key);

            if (objectClass == null)
            {
                return NotFound();
            }
            objectClass.ObjectState=ObjectState.Modified;
            patch.Patch(objectClass);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObjectClassExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objectClass);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            ObjectClass objectClass = await _objectClassService.FindAsync(key);

            if (objectClass == null)
            {
                return NotFound();
            }
            objectClass.ObjectState=ObjectState.Deleted;
            _objectClassService.Delete(objectClass);
            await Request.GetContext().SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (!Request.IsBatchRequest())
                {
                    Request.GetContext().Dispose();
                }
            }
            base.Dispose(disposing);
        }

        private bool ObjectClassExists(string className)
        {
            return _objectClassService.Query(e => e.ClassName == className).Select().Any();
        }
        private bool ObjectClassExists(long id)
        {
            return _objectClassService.Query(e => e.Id == id).Select().Any();
        }
    }
}