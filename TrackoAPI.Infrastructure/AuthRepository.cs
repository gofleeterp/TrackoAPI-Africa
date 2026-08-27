using EntityFramework.Extensions;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Owin.Security.Provider;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Migrations;
using System.Linq;
using System.Threading.Tasks;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Data;
using TrackoApi.MessageService;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using TrackoAPI.ViewModels;
using TrackoAPI.ViewModels.Global;

namespace TrackoAPI.Infrastructure
{
    public static class AuthRepoHelper
    {
        private static IEnumerable<ApiViewModule> GetInnerModules(this ApiViewModule apiViewModule)
        {
            IEnumerable<ApiViewModule> sm = null;
            foreach (var subModule in apiViewModule.SubModules)
            {
                sm = GetInnerModules(subModule);
            }
            return sm;
        }
    }

    public class AuthRepository : IDisposable, IAuthRepository
    {
        private readonly TrackoApiDbContext _db;
        private readonly IIdentityMessageService _emailService;
        private readonly IGlobalStore _gs;
        private readonly ISMSService _smsService;
        private readonly ApiUserManager _um;
        private readonly UserStore<ApiUser, ApiRole, long, ApiUserLogin, ApiUserRole, ApiUserClaim> _us;

        public AuthRepository(ITrackoApiDbContext _context, IUnitOfWorkAsync uow, IIdentityMessageService emailService, ISMSService smsService, IGlobalStore globalStore)
        {
            _db = (TrackoApiDbContext)_context;
            _us = new UserStore<ApiUser, ApiRole, long, ApiUserLogin, ApiUserRole, ApiUserClaim>(_db);
            _um = new ApiUserManager(_us, emailService, smsService, globalStore);
            _emailService = emailService;
            _smsService = smsService;
            _gs = globalStore;
        }

        public void AddAccessControls(ApiRolePermission acls)
        {
            try
            {
                _db.ApiAccessControls.Add(acls);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message, ex.InnerException);
            }
        }

        public async Task<bool> AuthorizeDevice(string deviceIdentity, string otp)
        {
            var device =
                await
                    _db.ApiDevices
                        .AsQueryable()
                        .FirstOrDefaultAsync(x => x.DeviceIdentity == deviceIdentity && x.OTP == otp);
            if (device != null)
            {
                device.ObjectState = ObjectState.Modified;
                device.IsVerified = true;
                if (device.DeviceOS == DeviceOS.Android ||
                    device.DeviceOS == DeviceOS.WindowsPhoneOS ||
                    device.DeviceOS == DeviceOS.iOS)
                {
                }
                return (await _db.SaveChangesAsync()) > 0;
            }
            return false;
        }

        public void Begin(IsolationLevel level = IsolationLevel.ReadCommitted)
        {
            if (_db.Database.CurrentTransaction == null)
            {
                _db.Database.BeginTransaction(level);
            }
        }

        public async Task<IdentityResult> ChangePassword(ChangePassword password)
        {
            return await _um.ChangePasswordAsync(Helper.GetLoggedInUserId(), password.OldPassword, password.NewPassword);
        }

        public void Commit()
        {
            _db.Database.CurrentTransaction?.Commit();
        }

        public async Task CreateSession(ApiSession session)
        {
            var ssn = _db.ApiSessions.Where(x => x.ApplicationId == session.ApplicationId && x.UserId == session.UserId && !x.EndDateTime.HasValue);
            if (ssn.Any())
            {
                await ssn.ForEachAsync(i => { i.EndDateTime = DateTime.UtcNow; i.ObjectState = ObjectState.Modified; });
            }
            session.UserId = session.UserId;
            var s = _db.ApiSessions.Add(session);
            await _db.SaveChangesAsync();
        }

        public async Task<IdentityResult> CreateUpdateRole(vwRole role)
        {
            if (await this.GetRoles().AnyAsync(x => x.Name == role.RoleName))
                return IdentityResult.Failed("Role with same name already Exists.");
            if (await this.GetRoles().AnyAsync(x => x.UserId.HasValue && x.Id == role.Id))
                return IdentityResult.Failed("Role is reserved, So cannot be updated.");
            if (role.Id > 0 && !await this.GetRoles().AnyAsync(x => x.Id == role.Id))
                return IdentityResult.Failed("Role does not exists in database");
            try
            {
                Begin();
                _db.Set<ApiRole>().AddOrUpdate(new ApiRole() { Id = role.Id, IsReserved = false, UserId = null, Name = role.RoleName });
                await _db.SaveChangesAsync();
                Commit();
                return IdentityResult.Success;
            }
            catch (Exception)
            {
                Rollback();
                throw;
            }
        }

        //public IQueryable<ApiRole> GetRoles(long userid)
        //{
        //    return _db.Roles.Include(c=>c.AccessList).Where(x => x.Users.Any(y => y.UserId == userid)).AsQueryable();//Users.Include(x => x.Roles).Where(y=>y.Id==userid).AsQueryable();
        //}
        public async Task<IdentityResult> CreateUpdateUser(RegisterUser userModel)
        {
            try
            {
                Begin();
                var user = (await _us.Users.Include(x => x.fk_Address).Include(x => x.Roles.Select(y => y.fk_Role)).FirstOrDefaultAsync(x => x.Id == userModel.Id)) ?? new ApiUser(userModel.UserName);
                if (userModel.UserName != user.UserName && user.Id > 0 && string.IsNullOrWhiteSpace(userModel.ConfirmPassword))
                {
                    return IdentityResult.Failed("When Changing User name, you should also change password");
                }

                #region Address Section

                user.fk_Address = user.fk_Address ?? new PostalAddress();
                var a = userModel.fk_Address;
                if (a != null)
                {
                    user.fk_Address.AddressLine1 = a.AddressLine1;
                    user.fk_Address.AddressLine2 = a.AddressLine2;
                    user.fk_Address.AddressLine3 = a.AddressLine3;
                    user.fk_Address.AltContactNumber = a.AltContactNumber;
                    user.fk_Address.AltContactPerson = a.AltContactPerson;
                    user.fk_Address.AltEmailAddress = a.AltEmailAddress;
                    user.fk_Address.CityId = a.CityId;
                    user.fk_Address.ContactNumber = a.ContactNumber;
                    user.fk_Address.ContactPerson = a.ContactPerson;
                    user.fk_Address.CountryId = a.CountryId;
                    user.fk_Address.EmailAddress = a.EmailAddress;
                    user.fk_Address.Landmark = a.Landmark;
                    user.fk_Address.ObjectState = user.fk_Address.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    user.fk_Address.StateId = a.StateId;
                    user.fk_Address.UnitNo = a.UnitNo;
                    if (user.AddressId.GetValueOrDefault(0) == 0) //New Address
                    {
                        user.AddressId = user.fk_Address.Id;
                        user.fk_Address.CreatedDOE = DateTime.Now;
                    }
                }

                #endregion Address Section

                user.FirstName = userModel.FirstName;
                user.LastName = userModel.LastName;
                user.MiddleName = userModel.MiddleName;
                user.JoinDate = userModel.JoiningDate;
                user.BirthDate = userModel.BirthDate;
                user.OfficeId = userModel.OfficeId;
                user.IsRoamingUser = userModel.IsRoaming;
                user.DefaultCashAccountId = userModel.DefaultCashAccountId;
                user.DefaultPumpAccountId = userModel.DefaultPumpAccountId;

                user.DefaultStoreAccountId = userModel.DefaultStoreAccountId;
                user.DefaultFleetManagerId = userModel.DefaultFleetManagerId;

                user.ReportingManagerId = userModel.ReportingManagerId;
                try
                {
                    //user.TypeId = (UserType)userModel.UserType;
                    user.TypeId = (UserType)Enum.ToObject(typeof(UserType), userModel.UserType);
                    //if (userModel.UserType == 200)
                    //{
                    //    user.TypeId = UserType.Admin;
                    //}
                    //else
                    //{
                    //    user.TypeId = UserType.User;
                    //}
                }
                catch (Exception)
                {
                    user.TypeId = UserType.User;
                }

                if (user.TypeId != UserType.Admin)
                {
                    var revokedRoles = user.Roles.Where(x => !userModel.Roles.Contains(x.RoleId) && !x.fk_Role.UserId.HasValue).ToList();
                    foreach (var role in revokedRoles)
                    {
                        user.Roles.Remove(role);
                    }
                    var oldsroleids = user.Roles.Select(x => x.RoleId).ToList();
                    var newRoles = userModel.Roles.Where(x => !oldsroleids.Contains(x));
                    foreach (var role in newRoles)
                    {
                        user.Roles.Add(new ApiUserRole() { RoleId = role, UserId = user.Id });
                    }
                }
                IdentityResult result;
                if (user.Id > 0)
                {
                    result = await _um.UpdateAsync(user);
                    if (!result.Succeeded)
                    {
                        Rollback();
                        return result;
                    }
                    if (!string.IsNullOrWhiteSpace(userModel.ConfirmPassword))
                    {
                        result = await _um.RemovePasswordAsync(user.Id);
                        if (!result.Succeeded)
                        {
                            Rollback();
                            return result;
                        }
                        result = await _um.AddPasswordAsync(user.Id, userModel.ConfirmPassword);
                        if (!result.Succeeded)
                        {
                            Rollback();
                            return result;
                        }
                    };
                }
                else
                {
                    result = await _um.CreateAsync(user, userModel.ConfirmPassword);
                    if (!result.Succeeded)
                    {
                        Rollback();
                        return result;
                    }
                }
                result = await CreateUpdatePrivateRole(user.Id, user.UserName);
                if (!result.Succeeded)
                {
                    Rollback();
                    return result;
                }
                Commit();
                return result;
            }
            catch (Exception)
            {
                Rollback();
                throw;
            }
        }

        public async Task<IdentityResult> DeleteRole(long id)
        {
            try
            {
                var role = this._db.Roles.Find(id);
                if (role == null) return IdentityResult.Failed("Role Already deleted");
                _db.Roles.Remove(role);
                var count = await _db.SaveChangesAsync();
                return count > 0 ? IdentityResult.Success : IdentityResult.Failed("No Role was deleted");
            }
            catch (Exception)
            {
                return IdentityResult.Failed("No Role was deleted and Error has been Reported to Technical Team.");
            }
        }

        public void Dispose()
        {
            //_db.Dispose();
            _um.Dispose();
        }

        public async Task<ApiUser> FindUserAsync(string userName, string password)
        {
            ApiUser user = null;
            try
            {
                user = await _um.FindAsync(userName, password);
            }
            catch (Exception ex)
            {
                throw;
            }
            if (user != null) return user;
            var userid = await _um.Users.Where(x => x.UserName == userName).Select(x => x.Id).FirstOrDefaultAsync();
            if (userid > 0)
            {
                await _um.AccessFailedAsync(userid);
            }
            return null;
        }

        public async Task<ApiUser> FindUserAsync(string userName, string deviceIdentity, int pin)
        {
            ApiUser user = null;
            try
            {
                var isuserRegistred = await _db.ApiDevices.Where(x => x.IsVerified && x.DeviceIdentity == deviceIdentity && x.PIN == pin && x.UserName == userName).AnyAsync();
                if (isuserRegistred)
                {
                    user = await _um.Users.Where(x => !x.IsSuspended && x.UserName == userName).FirstOrDefaultAsync();//TODO: Apply &&x.PhoneNumberConfirmed filter after SMS Service Implemented
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            return user;
        }

        public async Task<int> GetFianaceStatus()
        {
            int status = 0;
            int.TryParse((await _db.ApiConfigurations.FirstOrDefaultAsync(x => x.Key == "VoucherVisiblityFlag")).Value,
                        out status);
            return status;
        }

        public IQueryable<ApiRolePermission> GetObjectsByRoleId(long roleId) => _db.Set<ApiRolePermission>().Where(x => x.ApiRoleId == roleId).AsQueryable();

        public IQueryable<ApiRolePermission> GetObjectsByRoleIds(List<long> roleIds) => _db.Set<ApiRolePermission>().Where(x => roleIds.Contains(x.ApiRoleId)).AsQueryable();

        public IQueryable<ApiRole> GetRolePermissionByUserId(long userid) => _db.Roles.Include(c => c.AccessList).Where(x => x.Users.Any(y => y.UserId == userid)).AsQueryable();//Users.Include(x => x.Roles).Where(y=>y.Id==userid).AsQueryable();

        public IQueryable<ApiUserRole> GetRolesByUserId(long userid) => _db.Set<ApiUserRole>().Where(x => x.UserId == userid).AsQueryable();
        public async Task<bool> IsDeviceAuthorized(string deviceIdentity,string oldDeviceId)
        {
            if (await _db.ApiDevices.AsNoTracking()
                        .AsQueryable()
                        .AnyAsync(x => x.DeviceIdentity == deviceIdentity && x.IsVerified)) return true;
            if (string.IsNullOrWhiteSpace(oldDeviceId)) return false;
            var olddevice =await  _db.ApiDevices.FirstOrDefaultAsync(x => x.DeviceIdentity == oldDeviceId);
            if (olddevice == null|| !olddevice.IsVerified) return false;
            if (string.IsNullOrWhiteSpace(deviceIdentity)) return true;
            olddevice.DeviceIdentity = deviceIdentity;
            olddevice.ObjectState = ObjectState.Modified;
            await _db.SaveChangesAsync();
            Commit();
            return true;
        }

        public async Task<bool> IsIpAuthorized(long userId, string IpAddress)
        {
            return
                await
                    _db.IpUserMappings.AsNoTracking()
                        .AsQueryable()
                        .AnyAsync(x => x.IPAddress == IpAddress && x.UserId == userId);
        }

        public async Task<IdentityResult> ModifyRoleACL(long roleid, List<vwApiRolePermission> assigned)
        {
            if (assigned == null || !assigned.Any()) return IdentityResult.Success;
            try
            {
                Begin();
                var role = await _db.Roles.Include(x => x.AccessList).FirstOrDefaultAsync(x => x.Id == roleid);

                if (role == null)
                {
                    Rollback();
                    return IdentityResult.Failed("Role Not Found");
                }
                var dbset = _db.Set<ApiRolePermission>();
                //List<ApiRolePermission> newpermissionset=new List<ApiRolePermission>();
                foreach (var x in assigned)
                {
                    var entity = role?.AccessList?.FirstOrDefault(y => y.Id == x.Id) ?? new ApiRolePermission();
                    entity.Id = x.Id;
                    if (x.Id == 0 && x.Permission == 0) continue;
                    if (x.Id > 0 && x.Permission == 0)
                    {
                        entity.ObjectState = ObjectState.Deleted;
                        dbset.Remove(entity);
                        continue;
                    }
                    entity.ObjectState = x.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                    entity.ApiObjectId = x.ApiObjectId;
                    entity.ApiRoleId = role.Id;
                    entity.EntitySubTypeId = x.EntitySubTypeId;
                    entity.EntityType = x.EntityType;
                    entity.ObjectName = x.ObjectName;
                    entity.Permission = x.Permission;
                    entity.ApiRole = role;
                    //newpermissionset.Add(entity);
                    dbset.AddOrUpdate(entity);
                }
                //var assignedRoles = newpermissionset.GroupBy(x=>x.EntityType);

                //foreach (IGrouping<AclType, ApiRolePermission> gp1 in assignedRoles)
                //{
                //    var entityType = gp1.Key;
                //    foreach (IGrouping<long?, ApiRolePermission> gp2 in gp1.GroupBy(x=>x.EntitySubTypeId))
                //    {
                //        var assignedIds = gp2.Where(x => x.Id > 0).Select(x => x.Id);
                //        var entitysubtype = gp2.Select(x => x.EntitySubTypeId).FirstOrDefault();
                //        var revoiked =
                //            role.AccessList.Where(x => !assignedIds.Contains(x.Id) && x.EntitySubTypeId == entitysubtype&&x.EntityType== entityType);
                //        foreach (var permission in revoiked)
                //        {
                //            permission.ObjectState = ObjectState.Deleted;
                //            dbset.AddOrUpdate(permission);
                //        }
                //        foreach (ApiRolePermission permission in gp2)
                //        {
                //            permission.ObjectState = permission.Id > 0 ? ObjectState.Modified : ObjectState.Added;
                //            dbset.AddOrUpdate(permission);
                //        }
                //    }
                //}

                _db.Roles.AddOrUpdate(role);
                await _db.SaveChangesAsync();
                Commit();
                return IdentityResult.Success;
            }
            catch (Exception)
            {
                Rollback();
                return IdentityResult.Failed("Unable to Assign Permissions to Role");
            }
        }
        //public async Task<bool> RegisterDevice(string deviceIdentity,string computerName, string localIpAddress, string publicHostAddress,
        //    string remark)
        public async Task<bool> RegisterDeviceAsync(vwApiDevice vwDevice, string tenantEmail, string tenantName, string phoneNumber)
        {
            var user = await FindUserAsync(vwDevice.UserName, vwDevice.Password);
            if (user == null)
            {
                return false;
            }
            //_db.Users.AddOrUpdate(user);
            var deviceQuery = _db.ApiDevices.Where(x => x.UserName == vwDevice.UserName);
            deviceQuery = vwDevice.DeviceOSId != 0 ? deviceQuery.Where(x => x.ApplicationId == vwDevice.ApplicationId) : deviceQuery.Where(x => x.DeviceIdentity == vwDevice.DeviceIdentity);
            var existingDevices = await deviceQuery.ToListAsync();
            if (existingDevices.Any())
            {
                existingDevices.ForEach(x => x.ObjectState = ObjectState.Deleted);
                _db.ApiDevices.RemoveRange(existingDevices);
            }
            Random rnd = new Random();
            int otp = rnd.Next(1000, 9999);
            var device = new ApiDevice()
            {
                UserName = vwDevice.UserName,
                ComputerName = vwDevice.ComputerName,
                DeviceIdentity = vwDevice.DeviceIdentity,
                IsVerified = false,
                LocalHostIp = vwDevice.LocalHostIp,
                ObjectState = ObjectState.Added,
                PublicHostIp = vwDevice.PublicHostIp,
                Remark = vwDevice.Remark,
                OTP = otp.ToString(),
                ISP = vwDevice.ISP,
                Location = vwDevice.Location,
                DeviceOS = (DeviceOS)vwDevice.DeviceOSId.GetValueOrDefault(0),
                PIN = vwDevice.Pin,
                ApplicationId = vwDevice.ApplicationId
            };
            _db.ApiDevices.Add(device);
            var isSaved = await _db.SaveChangesAsync() > 0;
            if (!isSaved) return false;
            var message = new IdentityMessage()
            {
                Body = $"Dear Admin,<br/><br/>" +
                $"The <strong>{user.FirstName} {user.MiddleName} {user.LastName}</strong> has initiated a device registration request.<br/><br/>" +
                $"<strong>OTP:</strong> {device.OTP}<br/><br/>" +
                $"<strong>Device Details:</strong><br/>" +
                $"<ul>" +
                $"<li><strong>Tenant Name:</strong> {tenantName}</li>" +
                $"<li><strong>Device Name:</strong> {device.ComputerName}</li>" +
                $"<li><strong>User:</strong> {device.UserName}</li>" +
                $"<li><strong>Local Host (IP):</strong> {device.LocalHostIp}</li>" +
                $"<li><strong>Public Host (IP):</strong> {device.PublicHostIp}</li>" +
                $"<li><strong>ISP:</strong> {device.ISP}</li>" +
                $"<li><strong>User Message:</strong> {device.Remark}</li>" +
                $"</ul>" +
                $"If you did not request this registration or have any concerns, please contact the support team immediately.<br/><br/>" +
                $"Best regards,<br/>" +
                $"GOFLEET Africa",

                Destination = tenantEmail,
                Subject = $"OTP Request for - {tenantName} (User: {user.FirstName} {user.MiddleName} {user.LastName})."
            };
            await _emailService.SendAsync(message);
            if (_db.GetApiConfig<int>("IsSMSOTPEnabled") > 0 && !string.IsNullOrWhiteSpace(phoneNumber))
            {
                await _smsService.SendAsync(new IdentityMessage { Body = $"Your GoFleet OTP for User:{device.UserName} is {device.OTP}", Destination = phoneNumber });
            }
            return true;
        }

        public void Rollback()
        {
            _db.Database.CurrentTransaction?.Rollback();
        }
        public async Task<IdentityResult> SuspenUser(long userId)
        {
            var user = _um.Users.FirstOrDefault(x => x.Id == userId);
            if (user == null) return IdentityResult.Failed("User Not Found");
            var isRemoved = await _um.RemovePasswordAsync(userId);
            if (!isRemoved.Succeeded) return isRemoved;
            user.IsSuspended = true;
            var isSuspended = await _um.UpdateAsync(user);
            return isSuspended;
        }

        public async Task<bool> UnAuthorizeDevice(string deviceIdentity)
        {
            var device =
                await
                    _db.ApiDevices
                        .AsQueryable()
                        .FirstOrDefaultAsync(x => x.DeviceIdentity == deviceIdentity);
            if (device != null)
            {
                device.ObjectState = ObjectState.Modified;
                device.IsVerified = false;
                return await _db.SaveChangesAsync() > 0;
            }
            return false;
        }

        public IQueryable<UserDefinedReport> UserDefined()
        {
            return _db.UserDefinedReports.AsNoTracking().AsQueryable();
        }
        public IQueryable<ApiSession> Sessions=>_db.ApiSessions.AsNoTracking().AsQueryable();

        public IQueryable<ApiUser> Users()
        {
            return _us.Users;
        }

        private async Task<IdentityResult> CreateUpdatePrivateRole(long userid, string userName)
        {
            try
            {
                var role = await this.GetRoles().FirstOrDefaultAsync(x => x.UserId == userid) ?? new ApiRole();
                if (role.Id > 0 && role.Name != $"PrivateRole_{userName}")
                {
                    role.Name = $"PrivateRole_{userName}";
                    if (!(await this.GetRolesByUserId(userid).AnyAsync(x => x.RoleId == role.Id)))
                    {
                        role.Users.Add(new ApiUserRole() { RoleId = role.Id, UserId = userid, fk_Role = role });
                    }
                }
                if (role.Id == 0)
                {
                    role.Name = $"PrivateRole_{userName}";
                    role.UserId = userid;
                    role.IsReserved = false;
                    role.Users.Add(new ApiUserRole() { RoleId = role.Id, UserId = userid, fk_Role = role });
                }
                _db.Roles.AddOrUpdate(role);
                await _db.SaveChangesAsync();
                return IdentityResult.Success;
            }
            catch (Exception ex)
            {
                return IdentityResult.Failed($"Unable Create Private Role for User {userName}");
            }
        }
        #region TokenRefresh

        public IQueryable<ApiConfiguration> ApiConfigurations => _db.ApiConfigurations.AsNoTracking();

        public IQueryable<ClientConfiguration> ClientConfigurations => _db.ClientConfigurations.AsNoTracking();

        public async Task<bool> AddRefreshToken(ApiRefreshToken token)
        {
            var existingToken = _db.RefreshTokens.SingleOrDefault(r => r.Subject == token.Subject && r.ClientKey == token.ClientKey);
            if (existingToken != null)
            {
                var result = await RemoveRefreshToken(existingToken.Id);
            }
            _db.RefreshTokens.Add(token);
            return await _db.SaveChangesAsync() > 0;
        }
        public async Task<Tuple<bool,string>> IsVersionBugFree(string version,long? viewid)
        {
            if (string.IsNullOrEmpty(version)) return new Tuple<bool, string>(true,"");
            try
            {
                var fault = await _db.FaultVersions.Where(x => x.FaultyVersionCode == version && x.ViewId==viewid).FirstOrDefaultAsync();
                if(fault==null) return new Tuple<bool, string>(true, "");
                return new Tuple<bool, string>(false, string.IsNullOrWhiteSpace(fault.ErrorMessage)?$"The version you are using has some serious issues. We request you to either Rollback to Previous Version or if avialable, Upgrade  to latest version {fault.NewVersionCode}.":fault.ErrorMessage);
            }
            catch
            {
                return new Tuple<bool, string>(true, "");
            }
        }
        public async Task<ApiAppClient> FindClient(string appName, string screte, string key)
        {
            try
            {
                var client = await _db.Clients.Where(x => x.ApplicationId == appName && x.Secret == screte && x.ClientKey == key).FromCacheFirstOrDefaultAsync();
                return client;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
        public async Task<ApiRefreshToken> FindRefreshToken(string refreshTokenId)
        {
            var refreshToken = await _db.RefreshTokens.FindAsync(refreshTokenId);
            return refreshToken;
        }

        public IQueryable<ApiUser> FindUserById(long id) => _db.Users.Include(x => x.Roles).Where(y => y.Id == id).AsQueryable();

        public IQueryable<ApiRefreshToken> GetAllRefreshTokens() => _db.RefreshTokens;

        public IQueryable<ApiRole> GetRoles() => _db.Roles;

        public IQueryable<ApiViewModule> Modules() => _db.ApiModules.Include(x => x.SubModules).Include(x => x.Views).Where(x => x.Status == MasterStatus.Active);

        public async Task<bool> RemoveRefreshToken(string refreshTokenId)
        {
            var tenantId = Helper.LoggedInTenantId;
            //GlobalStore.Instance.AccessTokens.AddOrUpdate(tenantId, new List<string>(), (s, list) =>
            //{
            //    if (list == null) list = new List<string>();
            //    list?.Remove(refreshTokenId);
            //    return list;
            //});
            _gs.RemoveToken(tenantId, refreshTokenId);
            var userId = Helper.GetLoggedInUserId();
            if (userId > 0)
            {
                //GlobalStore.Instance.SignalRUsers.AddOrUpdate(tenantId, new List<ConnectedUser>(), (s, list) =>
                //{
                //    if (list == null) list = new List<ConnectedUser>();
                //    list?.RemoveAll(y => y.TenantId == tenantId && y.UserId == userId);
                //    return list;
                //});
                try
                {
                    _gs.RemoveUser(tenantId, userId);
                }
                catch
                {
                    //Ignore
                }
            }

            var refreshToken = await _db.RefreshTokens.FirstOrDefaultAsync(x => x.Id == refreshTokenId);
            if (refreshToken == null) return false;
            _db.RefreshTokens.Remove(refreshToken);
            return await _db.SaveChangesAsync() > 0;
        }
        public List<UserResourceResult> UserPermissions(long userId) => _db.Database.SqlQuery<UserResourceResult>($"Proc_GetUserResource {userId}").ToList();

        public IQueryable<ApiView> Views() => _db.ApiViews.Include(x => x.ApiViewModule).Where(x => x.ApiViewModule.Status == MasterStatus.Active);
        #endregion TokenRefresh
    }
}