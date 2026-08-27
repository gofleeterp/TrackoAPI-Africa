using Microsoft.Owin.Security;
using Microsoft.Owin.Security.OAuth;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Tenant.Models;
using TrackoApi.Core;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Models.Shared;
using Unity;

namespace TrackoAPI.Infrastructure.Providers
{
    public class SimpleOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly IUnityContainer _unity;

        private readonly IGlobalStore globalstore;
        //private UnityHierarchicalDependencyResolver _cont;

        //private string GetUniqueKey()
        //{
        //    Guid g = Guid.NewGuid();
        //    string GuidString = Convert.ToBase64String(g.ToByteArray());
        //    GuidString = GuidString.Replace("=", "");
        //    GuidString = GuidString.Replace("+", "");
        //    return GuidString;
        //}

        public SimpleOAuthProvider(IUnityContainer container)
        {
            _unity = container;
            globalstore = _unity.Resolve<IGlobalStore>();
        }

        public override Task GrantRefreshToken(OAuthGrantRefreshTokenContext context)
        {
            var originalClient = context.Ticket.Properties.Dictionary["client_id"];
            var currentClient = context.ClientId;
            if (originalClient != currentClient)
            {
                context.SetError("Refresh token is issued to a different clientId.");
                ////context.Rejected();
                return Task.FromResult<object>(null);
            }

            // Change auth ticket for refresh token requests
            var newIdentity = new ClaimsIdentity(context.Ticket.Identity);

            var newClaim = newIdentity.Claims.FirstOrDefault(c => c.Type == "newClaim");
            if (newClaim != null)
            {
                newIdentity.RemoveClaim(newClaim);
            }
            newIdentity.AddClaim(new Claim("newClaim", "newValue"));

            var newTicket = new AuthenticationTicket(newIdentity, context.Ticket.Properties);
            context.Validated(newTicket);

            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var allowedOrigin = context.OwinContext.Get<string>("as:clientAllowedOrigin") ?? "*";
            var tenantConnection = context.OwinContext.Get<string>("as:tenantid") ?? "";
            string clientName = context.OwinContext.Get<string>("clientName");
            context.OwinContext.Request.Headers.Add("Access-Control-Allow-Origin", new[] { allowedOrigin });
            var logType = context.OwinContext.Get<int>("LogType");
            var tenant = context.OwinContext.Get<TenantViewModel>("Tenant");
            var appupdateurl = context.OwinContext.Get<string>("AppUpdateUrl");
            var osname = context.OwinContext.Get<string>("osname");
            var hostname = context.OwinContext.Get<string>("hostname");
            var deviceid = context.OwinContext.Get<string>("deviceid");//appId
            var appid = context.OwinContext.Get<string>("appId");//appId
            var userHostAddress = HttpContext.Current.Request.UserHostAddress;
            AuthRepository auth = (AuthRepository)_unity.Resolve<IAuthRepository>();
            {
                try
                {
                    auth.Begin();
                    var session = new ApiSession()
                    {
                        ApplicationId = context.OwinContext.Get<string>("appId"),
                        StartDateTime = DateTime.Now,
                        Origin = allowedOrigin,
                        ObjectState = ObjectState.Added,
                        UserIp = userHostAddress,
                        HostName= hostname,
                        AppVersion = context.OwinContext.Get<string>("AppVersion"),
                        OSName = osname
                    };
                    ApiUser user = null;
                    var app = tenant.Apps.FirstOrDefault(x => x.ApplicationId == appid);
                    if (app != null && app.ApplicationType == ApplicationCategory.NativeMobileApp)
                    {
                        int pin = 0;
                        if (int.TryParse(context.Password, out pin))
                        {
                            user = await auth.FindUserAsync(context.UserName, deviceid, pin);
                        }
                        if (user == null)
                        {
                            auth.Rollback();
                            context.SetError("Unable to Authenticate, suspected causes are below.\n1)PIN was wrong.\n2)User has been suspended.\n3)Device has not been yet verified.");
                            return;
                        }
                        
                    }
                    else
                    {
                        user = await auth.FindUserAsync(context.UserName, context.Password);
                        if (user == null)
                        {
                            auth.Rollback();
                            context.SetError("The UserName or Password is incorrect.");

                            ////context.Rejected();
                            return;
                        }                        
                    }
                    if (user.IsSuspended)
                    {
                        auth.Rollback();
                        context.SetError("Unable to Authenticate, as the User has been suspended.");
                        return;
                    }
                    context.OwinContext.Set("userId", user.Id);
                    if (!await auth.IsIpAuthorized(user.Id, session.UserIp) && !user.IsRoamingUser)
                    {
                        auth.Rollback();
                        context.SetError("UnAuthorized Network.");
                        ////context.Rejected();
                        return;
                    }
                    session.UserId = user.Id;
                    await auth.CreateSession(session);
                    if (session.Id == 0)
                    {
                        auth.Rollback();
                        context.SetError("Unable to initiate new Session.");
                        ////context.Rejected();
                        return;
                    }
                    auth.Commit();
                    var financeAccountStatus = await auth.GetFianaceStatus();
                    var identity = new ClaimsIdentity(context.Options.AuthenticationType);
                    identity.AddClaim(new Claim("sub", user.UserName));
                    identity.AddClaim(new Claim("role", "user"));
                    identity.AddClaim(new Claim("TenantId", tenantConnection));
                    identity.AddClaim(new Claim("ClientKey", tenant.ClientKey));
                    identity.AddClaim(new Claim("SessionId", session.Id.ToString()));
                    identity.AddClaim(new Claim("ApplicationId", session.ApplicationId));
                    identity.AddClaim(new Claim("UserId", user.Id.ToString()));
                    identity.AddClaim(new Claim("LogType", (logType).ToString()));
                    identity.AddClaim(new Claim("UserType", ((int)user.TypeId).ToString()));
                    identity.AddClaim(new Claim("FinanceStatus", (financeAccountStatus).ToString()));
                    identity.AddClaim(new Claim("TenantEmailAddress", (tenant.EmailAddress)));
                    identity.AddClaim(new Claim("UserFullName", ($"{user.FirstName} {user.MiddleName} {user.LastName}")));
                    identity.AddClaim(new Claim("AppUpdateUrl", appupdateurl));
                    identity.AddClaim(new Claim("TenantShortName", tenant.ShortName));
                    identity.AddClaim(new Claim("TenantName", tenant.Name));
                    identity.AddClaim(new Claim("ConstCurTypeId", tenant.ConstCurTypeId.ToString()));
                    identity.AddClaim(new Claim("FormatUrl", app.FormatUrl??""));
                    identity.AddClaim(new Claim("SetupUrl", app.SetupUrl??""));

                    //var roles = auth.GetRoles(user.Item1.Id).Select(y => y.AccessList.Select(e=>new {e.EntityType,e.ApiObjectId,e.ObjectName}).Distinct()).ToArray();
                    //foreach (var acl in from bridge in roles from acl in bridge let acl1 = acl where !identity.Claims.Any(
                    //    x => x.Type == acl1.EntityType.ToString() && x.ValueType == acl1.ApiObjectId.ToString()) select acl)
                    //{
                    //    identity.AddClaim(new Claim(acl.EntityType.ToString(), acl.ObjectName, acl.ApiObjectId.ToString()));
                    //}
                    //TODO:Add All the Claims and Roles here from Db
                    var props = new AuthenticationProperties(new Dictionary<string, string>
                {
                    {"client_id",context.ClientId ?? string.Empty },
                    {"userName",user.UserName},
                    {"FirstName",string.IsNullOrWhiteSpace(user.FirstName)?"":user.FirstName },
                    {"LastName",string.IsNullOrWhiteSpace(user.LastName)?"":user.LastName },
                    {"MiddleName",string.IsNullOrWhiteSpace(user.MiddleName)?"":user.MiddleName},
                    {"SubscriberName",string.IsNullOrWhiteSpace(clientName)?"":clientName},
                    {"EmailAddress",string.IsNullOrWhiteSpace(tenant.EmailAddress)?"":tenant.EmailAddress},
                    {"TenantAddress",string.IsNullOrWhiteSpace(tenant.PostalAddress)?"":tenant.PostalAddress},
                    {"FinanceStatus",financeAccountStatus.ToString()},
                    {"SessionId",session.Id.ToString() },
                    {"UserType",((int)user.TypeId).ToString() },
                    {"PANNo", tenant.PANNo??""},
                    {"TenantId", tenant.Id},
                    {"AppUpdateUrl",appupdateurl },
                    {"FormatUrl",app.FormatUrl??"" },
                    {"SetupUrl",app.SetupUrl??"" }
                });
                    var ticket = new AuthenticationTicket(identity, props);
                    context.Validated(ticket);
                }
                catch (Exception e)
                {
                    auth.Rollback();
                    throw;
                }
            }
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            foreach (KeyValuePair<string, string> prop in context.Properties.Dictionary)
            {
                context.AdditionalResponseParameters.Add(prop.Key, prop.Value);
            }
            return Task.FromResult<object>(null);
        }

        public override Task TokenEndpointResponse(OAuthTokenEndpointResponseContext context)
        {
            return base.TokenEndpointResponse(context);
        }

        public override async Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            try
            {
                string appId = context.Parameters["applicationId"];//renamed from applicationName
                string version = context.Parameters["version"];
                string username = context.Parameters["username"];
                string osname = context.Parameters["osname"];
                string hostname = context.Parameters["hostname"];
                var deviceId = context.Parameters["deviceId"];
                var olddeviceId = context.Parameters["olddeviceId"];
                //var genmigrationScript = context.Parameters.Get("scriptmigration");
                if (bool.TryParse(context.Parameters.Get("scriptmigration"), out var genmigrationScript))
                {
                    context.OwinContext.Set("scriptmigration", genmigrationScript);
                }
                context.OwinContext.Set("appId", appId);

                ApiAppClient client;

                if (!context.TryGetBasicCredentials(out var clientId, out var clientSecret))
                {
                    context.TryGetFormCredentials(out clientId, out clientSecret);
                }

                if (context.ClientId == null)
                {
                    //Remove the comments from the below line context.SetError, and invalidate context
                    //if you want to force sending clientId/secrects once obtain access tokens.
                    //context.Validated();
                    context.SetError("Missing ClientId." + context.Parameters.Select(x => x.Value).JoinStrings(","));
                    ////context.Rejected();
                    return;
                }
                TenantViewModel tenant = null;
                tenant = !Helper.HostedOnPremise ? globalstore.GetOrAddTenant(clientId, GetTenantFromDb) : globalstore.GetOrAddTenant(clientId);
                if (tenant == null || tenant.Apps.All(x => x.ApplicationId != appId))
                {
                    context.SetError("You are not authorized to use this software.");
                    //context.Rejected();
                    return;
                }

                if (!tenant.IsActive)
                {
                    context.SetError("Your Services are suspended.\n Please contact GoFleet Administration.");
                    //context.Rejected();
                    return;
                }

                if (tenant.IsSingleUserMode && username != "sa")
                {
                    context.SetError("Services are in Single User Mode.");
                    //context.Rejected();
                    return;
                }
                if (tenant.Apps.Any(x => x.ApplicationType == ApplicationCategory.NativeConfidential || x.ApplicationType == ApplicationCategory.NativeMobileApp))
                {
                    if (string.IsNullOrWhiteSpace(clientSecret))
                    {
                        context.SetError("Missing Client secret.");
                        return;
                    }
                    if (tenant.Secret != clientSecret)
                    {
                        context.SetError("Client secret is invalid.");
                        return;
                    }
                }

                if (Helper.HostedOnPremise/*&&string.IsNullOrWhiteSpace(tenant.ConnectionString)*/)
                {
                    var connection = ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
                    if (string.IsNullOrWhiteSpace(connection)) throw new BusinessException(ErrorCode.GLB103, "Hosted OnPremise configuration not found");
                    tenant.ConnectionString = connection;
                }

                context.OwinContext.Set("as:tenantid", tenant.Id);
                context.OwinContext.Set("AppUpdateUrl", tenant.Apps.FirstOrDefault()?.UpdateUrl);
                context.OwinContext.Set("SetupUrl", tenant.Apps.FirstOrDefault()?.SetupUrl);
                context.OwinContext.Set("FormatUrl", tenant.Apps.FirstOrDefault()?.FormatUrl);
                context.OwinContext.Set("LogType", (int)tenant.LogType);
                context.OwinContext.Set("as:tenantConnection", tenant.ConnectionString);
                context.OwinContext.Set("Tenant", tenant);
                context.OwinContext.Set("ApplicationType", tenant.Apps.FirstOrDefault()?.ApplicationType);
                if (genmigrationScript)
                {
                    try
                    {
                        var script = TrackoApi.Data.GenerateMigrationScript.GenerateSqlScript(tenant.ConnectionString);
                        if (!string.IsNullOrWhiteSpace(script))
                        {
                            context.SetError($"Script has been Generated");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        context.SetError($"Unable to Generate Script.\n{ex.GetBaseException().Message}");
                        return;
                    }
                }
                try
                {
                    AuthRepository repo = (AuthRepository)_unity.Resolve<IAuthRepository>();
                    {
                        client = await repo.FindClient(appId, clientSecret, clientId);
                        if (client == null)
                        {
                            context.SetError($"Client '{context.ClientId}' is not registered in the system.");
                            return;
                        }

                        if (!string.IsNullOrWhiteSpace(client.MinimumSupportedVersion) &&
                            !string.IsNullOrWhiteSpace(version))
                        {
                            try
                            {
                                Version mcv = Version.Parse(client.MinimumSupportedVersion);                                
                                Version cv = Version.Parse(version);
                                if (cv < mcv)
                                {
                                    context.SetError("You are using an old version of ERP. Kindly upgrade to a newer version.");
                                    return;
                                }
                                if (!string.IsNullOrWhiteSpace(client.MaximumSupportedVersion)) {
                                    Version mxcv = Version.Parse(client.MaximumSupportedVersion);
                                    if (mxcv != null && cv > mxcv)
                                    {
                                        context.SetError($"The current version of ERP is not applicable for your company.\n\nPlease install the appropriate version {client.MaximumSupportedVersion}");
                                        return;
                                    } 
                                }
                            }
                            catch (Exception ex)
                            {
                                //ex.ToExceptionless().AddObject(new { MinimumVersion = client.MinimumSupportedVersion, ClientVersion = version }).Submit();
                            }
                        }
                        var faultCheck = await repo.IsVersionBugFree(version, null);
                        if (!(faultCheck?.Item1 ?? true))
                        {
                            context.SetError(string.IsNullOrWhiteSpace(faultCheck.Item2)? "The version you are using has some serious issues. We request you to either Rollback to Previous Version or if avialable, Upgrade  to latest version":faultCheck.Item2);
                            return;
                        }
                        if (!string.IsNullOrWhiteSpace(version))
                        {
                            context.OwinContext.Set("AppVersion", version);
                        }

                        if (!string.IsNullOrWhiteSpace(deviceId) && !await repo.IsDeviceAuthorized(deviceId,olddeviceId))
                        {
                            context.SetError($"Your Device is not registered with {tenant.Name}.");
                            return;
                        }
                    }
                    if (!client.IsActive)
                    {
                        context.SetError("Services to this software package are suspended.");
                        return;
                    }
                    context.OwinContext.Set("as:clientAllowedOrigin", client.AllowedOrigin);
                    context.OwinContext.Set("as:clientRefreshTokenLifeTime", client.RefreshTokenLifeTime.ToString());
                }
                catch (Exception e)
                {
                    if (!Helper.HostedOnPremise)
                    {
                        using (var db = new TenantDbContext())
                        {
                            var entity = new WebApiUsage
                            {
                                RequestMethod = context.Request.Method,
                                Uri = context.Request.Uri.OriginalString,
                                IP = HttpContext.Current.Request.UserHostAddress,
                                TenantKey = tenant.Id,
                                RequestTimestamp = DateTime.Now,
                                ApplicationKey = appId,
                                UserKey = 0,
                                RequestContent = $"Login Attempt by user {username}",
                                ResponseContent = e.ToString(),
                                ResponseStatusCode = 400,
                                ResponseTimestamp = DateTime.Now,
                            };
                            entity.RequestHeaders = entity.extractHeaders(context.Request.Headers.ToList());
                            db.ApiLog.Add(entity);
                            await db.SaveChangesAsync();
                        }
                    }

                    context.SetError(e.GetBaseException().Message);
                    return;
                }

                context.OwinContext.Set("osname", osname);
                context.OwinContext.Set("hostname", hostname);
                context.OwinContext.Set("deviceid", deviceId);
                context.OwinContext.Set("clientName", tenant.Name);

                context.Validated();
            }
            catch (BusinessException ex)
            {
                context.SetError(string.IsNullOrWhiteSpace(ex.ExtraInfo) ? ex.GetBaseException().Message : ex.ExtraInfo);
            }
            catch (Exception e)
            {
#if DEBUG
                context.SetError(e.GetBaseException().Message);
#else
                context.SetError("Something went wrong :( and has been reported to technical team.");
#endif
            }
        }

        private TenantViewModel GetTenantFromDb(string clientKey)
        {
            if (Helper.HostedOnPremise) return globalstore.GetOrAddTenant(clientKey);
            using (var dbcontext = new TenantDbContext())
            {
                return dbcontext.Tenants.Where(x => x.ClientKey == clientKey).Select(x =>
                    new TenantViewModel
                    {
                        Id = x.Id,
                        Name = x.Name,
                        PostalAddress = x.PostalAddress,
                        EmailAddress = x.EmailAddress,
                        Apps = x.Apps.Select(y => new TenantAppViewModel
                        {
                            ApplicationId = y.ApplicationId,
                            IsActive = y.IsActive,
                            AppName = y.fk_Application.ApplicationName,
                            ApplicationType = y.fk_Application.ApplicationType,
                            UpdateUrl = y.UpdateUrl,
                            NoOfUsers = y.NoOfActiveUsers,                            
                            FormatUrl = y.FormatUrl,
                            SetupUrl = y.SetupUrl
                        }).ToList(),
                        ConnectionString = x.ConnectionString,
                        LogType = x.LogType,
                        IsActive = x.IsActive,
                        IsSingleUserMode = x.IsSingleUserMode,
                        IsHostedOnPremise = x.IsHostedOnPremise,
                        ClientKey = x.ClientKey,
                        PANNo = x.PANNo,
                        Secret = x.Secret,
                        AccessCode = x.AccessCode,
                        ShortName = x.ShortName,
                        ServerUrl = x.ServerUrl,
                        PhoneNumber = x.PhoneNumber,
                        RemoteBackupPath = x.RemoteBackupPath,
                        WebAddress = x.WebAddress,
                        ConstCurTypeId=x.ConstCurTypeId
                    }).FirstOrDefault();
            }
        }
    }
}