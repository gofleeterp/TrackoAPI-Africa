using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Query;
using System.Web.OData.Routing;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Controllers
{
    [AuthorizeEx]
    public class ObjectCategoriesController:ODataController
    {
        private readonly IObjectCategoryService _objectCategoryService;

        public ObjectCategoriesController(IObjectCategoryService service)
        {
            _objectCategoryService = service;
        }
        // GET: odata/ObjectCategorys
        [HttpGet,EnableQuery]
        public IQueryable<ObjectCategory> Get()
        {
            return _objectCategoryService.Queryable();
        }

        [ODataRoute("GetCategoriesByReportId(reportId={reportId})"),HttpGet, EnableQuery]
        public IQueryable<ObjectCategory> GetReportCategories([FromODataUri]long reportId,ODataQueryOptions<ObjectCategory> options)
        {
            var uow = Request.GetContext();
            var rawroleids =
                uow.Repository<ReportParameter>()
                    .Queryable()
                    .Where(
                        x =>
                            x.ReportId == reportId && x.FieldTypeId == ReportParameterType.ListBox &&
                            x.EnumTypeId == 233)
                    .Select(x => x.RoleIds)
                    .FirstOrDefault();
            if(string.IsNullOrWhiteSpace(rawroleids))return new List<ObjectCategory>().AsQueryable();
            var roletypes = rawroleids.Split(';');
            
            var list=new List<string>();
            foreach (string roletype in roletypes)
            {
                var roles = roletype.Split(':');
                var rt = roles[0];
                var roleids = roles[1].Split(',').Select(x=>rt+":"+x).ToList();
               if(roleids.Any())list.AddRange(roleids);
            }
            return
                _objectCategoryService.Queryable()
                    .Where(x => x.CategoryTypeId == 1156 && list.Contains(x.RoleTypeId + ":" + (x.RoleTypeId==1146?x.RoleTypeId:x.RoleId)));
        }
        [ODataRoute("GetCategoriesByCustomReportId(reportId={reportId})"), HttpGet, EnableQuery]
        public IQueryable<ObjectCategory> GetCustomReportCategories([FromODataUri]long reportId, ODataQueryOptions<ObjectCategory> options)
        {
            var uow = Request.GetContext();
            var rawroleids =
                uow.Repository<UserDefinedReportParameter>()
                    .Queryable()
                    .Where(
                        x =>
                            x.ReportId == reportId && x.FieldTypeId == ReportParameterType.ListBox &&
                            x.EnumTypeId == 233)
                    .Select(x => x.RoleIds)
                    .FirstOrDefault();

            if (string.IsNullOrWhiteSpace(rawroleids)) return new List<ObjectCategory>().AsQueryable();
            var roletypes = rawroleids.Split(';');

            var list = new List<string>();
            foreach (string roletype in roletypes)
            {
                var roles = roletype.Split(':');
                var rt = roles[0];
                var roleids = roles[1].Split(',').Select(x => rt + ":" + x).ToList();
                if (roleids.Any()) list.AddRange(roleids);
            }
           return
                _objectCategoryService.Queryable()
                    .Where(x => x.CategoryTypeId == 1156 && list.Contains(x.RoleTypeId + ":" + x.RoleId));
        }

        [ODataRoute("GetReportingCategories")]
        public IQueryable<vwReportCategory> GetReportCategories()
        {
            var uow = Request.GetContext();
            var cat = _objectCategoryService.Queryable().Where(x=>x.IsVisibility);
            var gems = uow.RepositoryAsync<ConstantValue>().Queryable().Where(x=>x.ConstantTypeId== 44);//Generic Master Types
            var roles = uow.RepositoryAsync<ConstantValue>().Queryable();//Party Roles
            var groups = uow.RepositoryAsync<AccountGroup>().Queryable();//Account Groups
            var offices = uow.RepositoryAsync<OfficeMaster>().Queryable();//Account Groups
            //var acrt=new List<long>() {1145,1146};
            var generics = from c in cat
                join g in gems on c.RoleId equals g.Id
                where (c.RoleTypeId!=1145||c.RoleTypeId!=1146)
                select new vwReportCategory()
                {
                    Id = c.Id,
                    CategoryName = c.CategoryName,
                    CategoryType = c.fk_CategoryType.ConstantName,
                    CategoryTypeId = c.CategoryTypeId,
                    RoleTypeId = c.RoleTypeId,
                    RoleType = c.fk_RoleType.ConstantName,
                    RoleId = c.RoleId,
                    Role = g.ConstantName,
                    IsReserved = c.IsReserved
                };
            var partyroles= from c in cat
                          join a in roles on c.RoleId equals a.Id
                          where c.RoleTypeId== 1145
                            select new vwReportCategory()
                          {
                                Id = c.Id,
                                CategoryName = c.CategoryName,
                                CategoryType = c.fk_CategoryType.ConstantName,
                                CategoryTypeId = c.CategoryTypeId,
                                RoleTypeId = c.RoleTypeId,
                                RoleType = c.fk_RoleType.ConstantName,
                                RoleId = c.RoleId,
                                Role = a.ConstantName,
                                IsReserved = c.IsReserved
                            };
            var accountgroups = from c in cat
                             join a in groups on c.RoleId equals a.Id
                             where c.RoleTypeId == 1146
                             select new vwReportCategory()
                             {
                                 Id = c.Id,
                                 CategoryName = c.CategoryName,
                                 CategoryType = c.fk_CategoryType.ConstantName,
                                 CategoryTypeId = c.CategoryTypeId,
                                 RoleTypeId = c.RoleTypeId,
                                 RoleType = c.fk_RoleType.ConstantName,
                                 RoleId = c.RoleId,
                                 Role = a.GroupName,
                                 IsReserved = c.IsReserved
                             };
            var officeCats= from c in cat
                            //join a in offices on c.RoleId equals a.Id
                            where c.RoleTypeId ==1292
                            select new vwReportCategory()
                            {
                                Id = c.Id,
                                CategoryName = c.CategoryName,
                                CategoryType = c.fk_CategoryType.ConstantName,
                                CategoryTypeId = c.CategoryTypeId,
                                RoleTypeId = c.RoleTypeId,
                                RoleType = c.fk_RoleType.ConstantName,
                                RoleId = c.RoleId,
                                Role = c.RoleName,
                                IsReserved = c.IsReserved
                            };
            return generics.Union(partyroles).Union(accountgroups).Union(officeCats);
        }
        // GET: odata/ObjectCategories(5)
        [EnableQuery]
        public SingleResult<ObjectCategory> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_objectCategoryService.Queryable().Where(t => t.Id == key));
        }
        // PUT: odata/ObjectCategorys(5)
       public async Task<IHttpActionResult> Put(long key, ObjectCategory objectCategory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (key != objectCategory.Id)
            {
                return BadRequest();
            }
            objectCategory.ObjectState=ObjectState.Modified;
            _objectCategoryService.Update(objectCategory);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObjectCategoryExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objectCategory);
        }
        // POST: odata/ObjectCategorys
        public async Task<IHttpActionResult> Post(ObjectCategory objectCategory)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            objectCategory.ObjectState = ObjectState.Added;
            _objectCategoryService.Insert(objectCategory);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                if (ObjectCategoryExists(objectCategory.CategoryName))
                {
                    throw new BusinessException(ErrorCode.GLB104, "Record Already Exists");
                }
                throw;
            }

            return Created(objectCategory);
        }
        //// PATCH: odata/ObjectCategorys(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ObjectCategory> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            ObjectCategory objectCategory = await _objectCategoryService.FindAsync(key);

            if (objectCategory == null)
            {
                return NotFound();
            }
            objectCategory.ObjectState=ObjectState.Modified;
            patch.Patch(objectCategory);

            try
            {
                await Request.GetContext().SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ObjectCategoryExists(key))
                {
                    return NotFound();
                }
                throw;
            }

            return Updated(objectCategory);
        }
        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            ObjectCategory objectCategory = await _objectCategoryService.FindAsync(key);

            if (objectCategory == null)
            {
                return NotFound();
            }
            objectCategory.ObjectState=ObjectState.Deleted;
            _objectCategoryService.Delete(objectCategory);
            await Request.GetContext().SaveChangesAsync();

            return StatusCode(HttpStatusCode.NoContent);
        }
        // GET: odata/ObjectCategories(5)/Classes
        [EnableQuery]
        public IQueryable<ObjectClass> GetObjectClasses([FromODataUri] long key)
        {
            return _objectCategoryService.Queryable().Where(m => m.Id == key).SelectMany(m => m.ObjectClasses);
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

        private bool ObjectCategoryExists(string categoryName)
        {
            return _objectCategoryService.Query(e => e.CategoryName == categoryName).Select().Any();
        }
        private bool ObjectCategoryExists(long id)
        {
            return _objectCategoryService.Query(e => e.Id == id).Select().Any();
        }
        [HttpPost,ODataRoute("ObjectCategories({key})/ObjectClasses")]
        public async Task<IHttpActionResult> PostObjectClasses([FromODataUri] long key, ObjectClass entity)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var category =await _objectCategoryService.Queryable().FirstOrDefaultAsync(x => x.Id == key);
            if (category == null)
            {
                return NotFound();
            }
            entity.CategoryId = key;
            entity.Category = category;
            entity.ObjectState = ObjectState.Added;
            Request.GetContext().RepositoryAsync<ObjectClass>().Insert(entity);
            await Request.GetContext().SaveChangesAsync();
            return Created(entity);
        }
    }
}