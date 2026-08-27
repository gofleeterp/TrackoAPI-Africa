using EntityFramework.Caching;
using EntityFramework.Extensions;
using Microsoft.AspNet.Identity;
using Repository.Pattern.Ef6.Extentions;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web.Http;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Global;
using TrackoApi.Service;
using TrackoAPI.Infrastructure;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels;

namespace TrackoAPI.Controllers
{
    [RoutePrefix("api/ApiSecurity"), AuthorizeEx]
    public class AuthenticationController : ApiController
    {
        private readonly IPostalAddressService _addressService;
        private readonly IAuthRepository _auth;
        private readonly IGlobalStore _gStore;

        public AuthenticationController(IAuthRepository repo, IPostalAddressService addressService, IGlobalStore globalStore)
        {
            _auth = repo;
            _addressService = addressService;
            _gStore = globalStore;
        }

        [Route("AssignResource"), AuthorizeEx]
        public IHttpActionResult AssignResource(ApiRolePermission acl)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            _auth.AddAccessControls(acl);
            return Ok();
        }

        [Route("ChangePassword"), AuthorizeEx]
        public async Task<IHttpActionResult> ChangePassword(ChangePassword data)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (!data.NewPassword.Equals(data.ConfirmNewPassword))
                return BadRequest("New password and Confirmed new password do not match.");
            if (data.NewPassword.Equals(data.OldPassword))
                return BadRequest("New password should not match old password.");

            var result = await _auth.ChangePassword(data);
            var errorResult = GetErrorResult(result);
            return errorResult ?? Ok();
        }

        [Route("heartbeat({token})"), HttpGet, AuthorizeEx]
        public IHttpActionResult CheckHeartBeat([FromUri]string token)
        {
            try
            {
                var hash = Helper.GetHash(token);
                var tenantid = Helper.LoggedInTenantId;
                //return Ok(new { IsAlive = /*_auth.GetAllRefreshTokens().Any(x => x.Id == hash)*/ GlobalStore.Instance.AccessTokens.ContainsKey(tenantid)&& GlobalStore.Instance.AccessTokens[tenantid].Contains(hash) });
                return Ok(new { IsAlive = _gStore.IsTokenExists(tenantid, hash) });
            }
            catch {
                return Ok(new { IsAlive = true }); ; 
            }
        }

        [HttpGet, Route("CleanRadisCache"), AuthorizeEx]
        public IHttpActionResult CleanRadisCache()
        {
            //CacheManager.Current.Clear();
            _gStore.CleanRadisCache(Helper.LoggedInTenantId);
            return Ok();
        }

        [HttpGet, Route("ClearAllRadisCache"), AuthorizeEx]
        public IHttpActionResult ClearAllRadisCache()
        {
            //CacheManager.Current.Clear();
            _gStore.CleanRadisCache();
            return Ok();
        }

        [HttpGet, Route("ClearCache"), AuthorizeEx]
        public IHttpActionResult ClearAppCache()
        {
            //CacheManager.Current.Clear();
            CacheManager.Current.Expire(Helper.LoggedInTenantId);
            CacheManager.Current.Expire("Global");
            _gStore.CleanRadisCache(Helper.LoggedInTenantId);
            DbRuleExtenation.ClearCache();
            return Ok();
        }

        [Route("CreateUserRole"), AuthorizeEx]
        public async Task<IHttpActionResult> CreateUserRole(vwRole role)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _auth.CreateUpdateRole(role);
            var errorResult = GetErrorResult(result);
            return errorResult ?? Ok(role);
        }

        [Route("DeleteRole({id})"), AuthorizeEx, HttpDelete]
        public async Task<IHttpActionResult> DeleteRole([FromUri]long id)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _auth.DeleteRole(id);
            var errorResult = GetErrorResult(result);
            return errorResult ?? StatusCode(HttpStatusCode.NoContent);
        }

        [AuthorizeEx]
        [Route("GetAllTokens")]
        public IHttpActionResult Get()
        {
            return Ok(_auth.GetAllRefreshTokens().Select(x => new { x.Id, x.ClientKey, x.ExpiresUtc, x.IssuedUtc, x.Subject }));
        }

        [AuthorizeEx, Route("GetAllocatedResource")]
        public IHttpActionResult GetAllocatedResource()
        {
            try
            {
                var exclude = new List<AclType> { AclType.MobileView, AclType.WebView };
                var modules = _auth.Modules().Where(x => x.Status == MasterStatus.Active).Select(x =>
               new
               {
                   x.Id,
                   x.DisplayText,
                   x.ModuleName,
                   x.ShortKey,
                   x.ParentModuleId,
                   x.ToolTipText,
                   x.DisplayOrder,
                   SubModules = x.SubModules.Where(e => e.Status == MasterStatus.Active).Select(y => new
                   {
                       y.Id,
                       y.ModuleName
                   })
               }).OrderBy(x => x.DisplayOrder).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(10)));
                var userId = this.GetClaimByKey<long>("UserId");
                if (this.GetClaimByKey<int>("UserType") == 200 || this.GetClaimByKey<int>("UserType") == 100)
                {
                    var allview = from y in _auth.Views().Where(x => x.Status == MasterStatus.Active&& !exclude.Contains(x.EntityType))
                                  select new
                                  {
                                      ObjectId = y.Id,
                                      y.EntityType,
                                      UserId = userId,
                                      ObjectName = y.Name,
                                      Permission = 15,
                                      y.DisplayText,
                                      y.IconName,
                                      y.ModuleId,
                                      y.ShortKey,
                                      y.ToolTipText,
                                      y.ApiViewModule.ModuleName,
                                      y.DisplayOrder
                                  };
                    var allUDR = _auth.UserDefined().Select(y => new
                    {
                        ObjectId = y.Id,
                        EntityType = AclType.UserDefinedReport,
                        UserId = userId,
                        ObjectName = y.Name,
                        Permission = 15,
                        DisplayText = y.Name,
                        y.fk_ParentReport.IconName,
                        y.fk_ParentReport.ModuleId,
                        y.fk_ParentReport.ShortKey,
                        y.fk_ParentReport.ToolTipText,
                        y.fk_ParentReport.ApiViewModule.ModuleName,
                        y.fk_ParentReport.DisplayOrder
                    });
                    return Ok(new { Permissions = allview.Union(allUDR).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(10))), Modules = modules });
                }

                var permissions = _auth.GetRoles().Include(x => x.AccessList).SelectMany(x => x.AccessList, (parent, child)
                    => new
                    {
                        ObjectId = child.ApiObjectId,
                        child.EntityType,
                        child.ObjectName,
                        child.Permission,
                        parent.Users
                    }).SelectMany(p => p.Users, (p1, c1)
                   => new
                   {
                       c1.UserId,
                       p1.Permission,
                       p1.EntityType,
                       p1.ObjectId,
                       p1.ObjectName
                   }).Where(o => o.UserId == userId && !exclude.Contains(o.EntityType)).OrderByDescending(x => x.Permission)
                    .GroupBy(g => new { g.UserId, g.ObjectId, g.EntityType, g.ObjectName },
                        (key, gg) =>
                            new
                            {
                                key.UserId,
                                Permission = gg.Max(g => g.Permission),
                                key.EntityType,
                                key.ObjectId,
                                key.ObjectName
                            })
                    .Distinct();
                var views = _auth.Views().Where(x => x.Status == MasterStatus.Active);
                var list = from x in permissions
                           join y in views on x.ObjectId equals y.Id
                           where x.EntityType == y.EntityType
                           select new
                           {
                               x.ObjectId,
                               x.EntityType,
                               x.UserId,
                               x.ObjectName,
                               x.Permission,
                               y.DisplayText,
                               y.IconName,
                               y.ModuleId,
                               y.ShortKey,
                               y.ToolTipText,
                               y.ApiViewModule.ModuleName,
                               y.DisplayOrder
                           };
                var udr = from x in permissions
                          join y in _auth.UserDefined() on x.ObjectId equals y.Id
                          where x.EntityType == AclType.UserDefinedReport
                          select new
                          {
                              x.ObjectId,
                              x.EntityType,
                              x.UserId,
                              x.ObjectName,
                              x.Permission,
                              DisplayText = y.Name,
                              y.fk_ParentReport.IconName,
                              y.fk_ParentReport.ModuleId,
                              y.fk_ParentReport.ShortKey,
                              y.fk_ParentReport.ToolTipText,
                              y.fk_ParentReport.ApiViewModule.ModuleName,
                              y.fk_ParentReport.DisplayOrder
                          };
                var userUDR = _auth.UserDefined().Where(x => x.UserId == userId || x.UserId == null).Select(y => new
                {
                    ObjectId = y.Id,
                    EntityType = AclType.UserDefinedReport,
                    UserId = userId,
                    ObjectName = y.Name,
                    Permission = 15,
                    DisplayText = y.Name,
                    y.fk_ParentReport.IconName,
                    y.fk_ParentReport.ModuleId,
                    y.fk_ParentReport.ShortKey,
                    y.fk_ParentReport.ToolTipText,
                    y.fk_ParentReport.ApiViewModule.ModuleName,
                    y.fk_ParentReport.DisplayOrder
                });

                return Ok(new { Permissions = list.Union(udr).Union(userUDR), Modules = modules });
            }
            catch (Exception ex)
            {
                throw new BusinessException(ErrorCode.GLB102, "Unable to get User Permissions");
            }
        }

        [AuthorizeEx, Route("GetAllRoles")]
        public IHttpActionResult GetAllRoles()
        {
            var roles = _auth.GetRoles().Where(x => !x.UserId.HasValue).Select(x => new
            {
                Id = x.Id,
                RoleName = x.Name
            });
            return Ok(roles.ToList());
        }

        [ClaimsAuthorize(ClaimType = "Entity", ClaimValue = "ReadClaims")]
        [AuthorizeEx, Route("claims")]
        public IHttpActionResult GetClaims()
        {
            var identity = User.Identity as ClaimsIdentity;

            var claims = from c in identity.Claims
                         select new
                         {
                             subject = c.Subject.Name,
                             type = c.Type,
                             value = c.Value
                         };
            return Ok(claims);
        }

        [AuthorizeEx, Route("GetClientConfiguration")]
        public IHttpActionResult GetClientConfiguration()
        {
            return Ok(_auth.ClientConfigurations.Select(x => new
            {
                Key = x.Id,
                Value = x.ConfigValue,
                x.Options
            }).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(10))));
        }

        [AuthorizeEx, Route("GetConfiguration")]
        public IHttpActionResult GetConfiguration()
        {
            return Ok(_auth.ApiConfigurations.Select(x => new
            {
                x.Key,
                x.Value,
                x.Options
            }).FromCache(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(10))));
        }

        [AuthorizeEx, Route("GetMobilePermissions")]
        public async Task<IHttpActionResult> GetMobileUserPermissions()
        {
            var userId = this.GetClaimByKey<long>("UserId");
            try
            {
                var exclude = new List<AclType> { AclType.Form, AclType.Report, AclType.UserControl, AclType.UserDefinedReport, AclType.WebView, AclType.AccontRole, AclType.AccountGroup, AclType.Vehicle };
                if (this.GetClaimByKey<int>("UserType") == 200 || this.GetClaimByKey<int>("UserType") == 100)
                {
                    var allview = await (from y in _auth.Views().Where(x => x.Status == MasterStatus.Active && !exclude.Contains(x.EntityType))
                                         select new
                                         {
                                             ObjectId = y.Id,
                                             y.EntityType,
                                             UserId = userId,
                                             ObjectName = y.Name,
                                             Permission = 15,
                                             y.DisplayText,
                                             y.IconName,
                                             y.ModuleId,
                                             y.ShortKey,
                                             y.ToolTipText,
                                             ModuleName = y.ApiViewModule.ModuleName,
                                             y.DisplayOrder
                                         }).ToListAsync().ConfigureAwait(false);
                    return Ok(allview);
                }

                var permissions = _auth.GetRoles().Include(x => x.AccessList).SelectMany(x => x.AccessList, (parent, child)
                    => new
                    {
                        ObjectId = child.ApiObjectId,
                        child.EntityType,
                        child.ObjectName,
                        child.Permission,
                        parent.Users
                    }).SelectMany(p => p.Users, (p1, c1)
                   => new
                   {
                       c1.UserId,
                       p1.Permission,
                       p1.EntityType,
                       p1.ObjectId,
                       p1.ObjectName
                   }).Where(o => o.UserId == userId && !exclude.Contains(o.EntityType)).OrderByDescending(x => x.Permission)
                    .GroupBy(g => new { g.UserId, g.ObjectId, g.EntityType, g.ObjectName },
                        (key, gg) =>
                            new
                            {
                                key.UserId,
                                Permission = gg.Max(g => g.Permission),
                                key.EntityType,
                                key.ObjectId,
                                key.ObjectName
                            })
                    .Distinct();
                var views = _auth.Views().Where(x => x.Status == MasterStatus.Active);
                var list = await (from x in permissions
                                  join y in views on x.ObjectId equals y.Id
                                      into objpermissions
                                  from cco in objpermissions.DefaultIfEmpty()
                                  select new
                                  {
                                      x.ObjectId,
                                      x.EntityType,
                                      x.UserId,
                                      x.ObjectName,
                                      x.Permission,
                                      DisplayText = cco != null ? cco.DisplayText : null,
                                      IconName = cco != null ? cco.IconName : null,
                                      ModuleId = cco != null ? cco.ModuleId : 0,
                                      ShortKey = cco != null ? cco.ShortKey : null,
                                      ToolTipText = cco != null ? cco.ToolTipText : null,
                                      ModuleName = cco != null ? cco.ApiViewModule.ModuleName : null,
                                      DisplayOrder = cco != null ? cco.DisplayOrder : 0
                                  }).ToListAsync().ConfigureAwait(false);

                return Ok(list);
            }
            catch (Exception)
            {
                throw new BusinessException(ErrorCode.GLB102, "Unable to get User Permissions");
            }
        }
        [AuthorizeEx, Route("GetWebPermissions")]
        public async Task<IHttpActionResult> GetWebUserPermissions()
        {
            var userId = this.GetClaimByKey<long>("UserId");
            try
            {
                var exclude = new List<AclType> { AclType.Form, AclType.Report, AclType.UserControl, AclType.UserDefinedReport,AclType.MobileView };
                if (this.GetClaimByKey<int>("UserType") == 200 || this.GetClaimByKey<int>("UserType") == 100)
                {
                    var allview = await (from y in _auth.Views().Where(x => x.Status == MasterStatus.Active && !exclude.Contains(x.EntityType))
                                         select new
                                         {
                                             ObjectId = y.Id,
                                             y.EntityType,
                                             UserId = userId,
                                             ObjectName = y.Name,
                                             Permission = 15,
                                             y.DisplayText,
                                             y.IconName,
                                             y.ModuleId,
                                             y.ShortKey,
                                             y.ToolTipText,
                                             ModuleName = y.ApiViewModule.ModuleName,
                                             y.DisplayOrder
                                         }).ToListAsync().ConfigureAwait(false);
                    return Ok(allview);
                }

                var permissions = _auth.GetRoles().Include(x => x.AccessList).SelectMany(x => x.AccessList, (parent, child)
                    => new
                    {
                        ObjectId = child.ApiObjectId,
                        child.EntityType,
                        child.ObjectName,
                        child.Permission,
                        parent.Users
                    }).SelectMany(p => p.Users, (p1, c1)
                   => new
                   {
                       c1.UserId,
                       p1.Permission,
                       p1.EntityType,
                       p1.ObjectId,
                       p1.ObjectName
                   }).Where(o => o.UserId == userId && !exclude.Contains(o.EntityType)).OrderByDescending(x => x.Permission)
                    .GroupBy(g => new { g.UserId, g.ObjectId, g.EntityType, g.ObjectName },
                        (key, gg) =>
                            new
                            {
                                key.UserId,
                                Permission = gg.Max(g => g.Permission),
                                key.EntityType,
                                key.ObjectId,
                                key.ObjectName
                            })
                    .Distinct();
                var views = _auth.Views().Where(x => x.Status == MasterStatus.Active);
                var list = await (from x in permissions
                                  join y in views on x.ObjectId equals y.Id
                                      into objpermissions
                                  from cco in objpermissions.DefaultIfEmpty()
                                  select new
                                  {
                                      x.ObjectId,
                                      x.EntityType,
                                      x.UserId,
                                      x.ObjectName,
                                      x.Permission,
                                      DisplayText = cco != null ? cco.DisplayText : null,
                                      IconName = cco != null ? cco.IconName : null,
                                      ModuleId = cco != null ? cco.ModuleId : 0,
                                      ShortKey = cco != null ? cco.ShortKey : null,
                                      ToolTipText = cco != null ? cco.ToolTipText : null,
                                      ModuleName = cco != null ? cco.ApiViewModule.ModuleName : null,
                                      DisplayOrder = cco != null ? cco.DisplayOrder : 0
                                  }).ToListAsync().ConfigureAwait(false);

                return Ok(list);
            }
            catch (Exception)
            {
                throw new BusinessException(ErrorCode.GLB102, "Unable to get User Permissions");
            }
        }

        public IHttpActionResult GetModuleNodes()
        {
            return Ok();
        }

        [AuthorizeEx, Route("GetPermissionsByRoleId({roleid})"), HttpGet]
        public IHttpActionResult GetPermissionsByRoleId([FromUri] long roleid)
        {
            var roles = _auth.GetObjectsByRoleId(roleid).Select(x => new vwApiRolePermission()
            {
                Id = x.Id,
                EntitySubTypeId = x.EntitySubTypeId,
                EntityType = x.EntityType,
                ApiObjectId = x.ApiObjectId,
                ObjectName = x.ObjectName,
                ApiRoleId = x.ApiRoleId,
                Permission = x.Permission
            });
            return Ok(roles.ToList());
        }

        [Route("GetUser({userId})"), AuthorizeEx]
        public async Task<IHttpActionResult> GetUser([FromUri]long userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _auth.Users().Where(x => x.Id == userId).Select(x => new RegisterUser
            {
                Id = x.Id,
                OfficeId = x.OfficeId,
                BirthDate = x.BirthDate,
                FirstName = x.FirstName,
                JoiningDate = x.JoinDate,
                LastName = x.LastName,
                MiddleName = x.MiddleName,
                OfficeName = x.fk_Office != null ? x.fk_Office.OfficeName : null,
                ReportingManager = x.fk_ReportingManager != null ? (x.fk_ReportingManager.FirstName + " " + x.fk_ReportingManager.MiddleName + " " + x.fk_ReportingManager.LastName) : null,
                ReportingManagerId = x.ReportingManagerId,
                AddressId = x.AddressId,
                UserName = x.UserName,
                IsRoaming = x.IsRoamingUser,
                UserType = (int)x.TypeId,

                DefaultCashAccountId = x.DefaultCashAccountId,
                DefaultPumpAccountId = x.DefaultPumpAccountId,

                DefaultStoreAccountId = x.DefaultStoreAccountId,
                DefaultFleetManagerId = x.DefaultFleetManagerId
            }).FirstOrDefaultAsync();
            if (result == null) return StatusCode(HttpStatusCode.NotFound);

            if (result.AddressId > 0)
            {
                var address = await
                    _addressService.Queryable().Where(x => x.Id == result.AddressId)
                        .Include(x => x.fk_State)
                        .Include(x => x.fk_City)
                        .Select(x => new vwPostalAddress
                        {
                            Id = x.Id,
                            CountryId = x.CountryId,
                            EmailAddress = x.EmailAddress,
                            ContactNumber = x.ContactNumber,
                            CityId = x.CityId,
                            StateId = x.StateId,
                            AltEmailAddress = x.AltEmailAddress,
                            UnitNo = x.UnitNo,
                            AltContactPerson = x.AltContactPerson,
                            Landmark = x.Landmark,
                            ContactPerson = x.ContactPerson,
                            AddressLine1 = x.AddressLine1,
                            AddressLine3 = x.AddressLine3,
                            CountryName = x.fk_Country != null ? x.fk_Country.CountryName : null,
                            AltContactNumber = x.AltContactNumber,
                            AddressLine2 = x.AddressLine2,
                            CityName = x.fk_City != null ? x.fk_City.CityName : null,
                            StateName = x.fk_State != null ? x.fk_State.Name : null
                        }).FirstOrDefaultAsync();
                if (address != null)
                {
                    result.fk_Address = address;
                }
            }
            var roles = _auth.GetRolesByUserId(userId).Select(x => x.RoleId).ToList();
            if (roles.Any())
            {
                result.Roles = roles;
            }
            return Ok(result);
        }

        [Route("IsAuthorized"), AuthorizeEx]
        public IHttpActionResult IsAuthorized()
        {
            return Ok();
        }
        [Route("IsSessionOwner({sessionId})"), AuthorizeEx]
        public async Task<IHttpActionResult> IsSessionOwner([FromUri]long sessionId)
        {
            var userId = Helper.GetLoggedInUserId();
            return Ok(await _auth.Sessions.AnyAsync(x => x.Id == sessionId&&x.UserId==userId));
        }
        [Route("quit({token})"), HttpGet, AuthorizeEx]
        public async Task<IHttpActionResult> LogOut([FromUri]string token)
        {
            var hash = Helper.GetHash(token);
            var result = await _auth.RemoveRefreshToken(hash);
            return Ok(result);
        }

        [Route("Me"), AuthorizeEx, HttpGet]
        public async Task<IHttpActionResult> Me()
        {
            var userid = this.GetClaimByKey<long>("UserId");
            return await GetUser(userid);
        }

        [Route("ModifyRoleACL({roleid})"), AuthorizeEx]
        public async Task<IHttpActionResult> ModifyRoleACL([FromUri]long roleid, List<vwApiRolePermission> assigned)
        {
            var result = await _auth.ModifyRoleACL(roleid, assigned);
            var errorResult = GetErrorResult(result);
            return errorResult ?? Ok();
        }

        [HttpGet, Route("RefreshRadisCache"), AuthorizeEx]
        public IHttpActionResult RefreshRadisCache()
        {
            //CacheManager.Current.Clear();
            _gStore.RefreshRadisCache(Helper.LoggedInTenantId);
            return Ok();
        }
        [Route("Register"), AuthorizeEx]
        public async Task<IHttpActionResult> Register(RegisterUser user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _auth.CreateUpdateUser(user);
            var errorResult = GetErrorResult(result);
            return errorResult ?? Ok(user);
        }

        [Route("RevokeToken"), AuthorizeEx]
        public async Task<IHttpActionResult> RevokeToken(string tokenId)
        {
            var result = await _auth.RemoveRefreshToken(tokenId);
            if (result)
            {
                return Ok();
            }
            return BadRequest("Token Id does not exist");
        }

        [Route("SuspendUser({userId})"), AuthorizeEx, HttpDelete]
        public async Task<IHttpActionResult> SuspendUser([FromUri]long userId)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var result = await _auth.SuspenUser(userId);
            var errorResult = GetErrorResult(result);
            return errorResult ?? StatusCode(HttpStatusCode.NoContent);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _auth.Dispose();
            }

            base.Dispose(disposing);
        }

        private IHttpActionResult GetErrorResult(IdentityResult result)
        {
            if (result == null)
            {
                return InternalServerError();
            }

            if (result.Succeeded) return null;
            if (result.Errors != null)
            {
                foreach (string error in result.Errors)
                {
                    ModelState.AddModelError("", error);
                }
            }

            if (ModelState.IsValid)
            {
                // No ModelState errors are available to send, so just return an empty BadRequest.
                return BadRequest();
            }

            return BadRequest(ModelState);
        }
    }
}