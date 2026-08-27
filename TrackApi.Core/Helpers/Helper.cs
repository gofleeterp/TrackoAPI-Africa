using Microsoft.Owin.BuilderProperties;
using Owin;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Web;

namespace TrackoApi.Core.Helpers
{

    public static class Helper
    {
        public static string CSharpName(this Type type)
        {
            var sb = new StringBuilder();
            var name = type.Name;
            if (!type.IsGenericType) return name;
            sb.Append(name.Substring(0, name.IndexOf('`')));
            sb.Append("<");
            sb.Append(string.Join(", ", type.GetGenericArguments()
                                            .Select(t => t.CSharpName())));
            sb.Append(">");
            return sb.ToString();
        }
        public static BusinessException GetBusinessException(this Exception exception)
        {
            if (exception == null)
                return null;
            Exception exception1 = exception;
            
            do
            {
               if(exception1 is BusinessException be)
                {
                    return be;
                }
                else
                {
                    exception1 = exception1.InnerException;
                }
            } while (true && exception1 != null);
            return null;
        }
        public static IList ToDynamic(this DataTable dt)
        {
            var dynamicDt = new List<dynamic>();
            foreach (DataRow row in dt.Rows)
            {
                dynamic dyn = new ExpandoObject();
                dynamicDt.Add(dyn);
                foreach (DataColumn column in dt.Columns)
                {
                    var dic = (IDictionary<string, object>)dyn;
                    dic[column.ColumnName] = row[column];
                }
            }
            return dynamicDt;
        }

        private sealed class DynamicRow : DynamicObject
        {
            private readonly DataRow _row;

            internal DynamicRow(DataRow row) { _row = row; }

            // Interprets a member-access as an indexer-access on the 
            // contained DataRow.
            public override bool TryGetMember(GetMemberBinder binder, out object result)
            {
                var retVal = _row.Table.Columns.Contains(binder.Name);
                result = retVal ? _row[binder.Name] : null;
                return retVal;
            }
        }
        // all params are optional
        public static void ChangeDatabase(
            this DbContext source,
            string initialCatalog = "",
            string dataSource = "",
            string userId = "",
            string password = "",
            bool integratedSecuity = true,
            string configConnectionStringName = "")
        /* this would be used if the
        *  connectionString name varied from 
        *  the base EF class name */
        {
            try
            {
                // use the const name if it's not null, otherwise
                // using the convention of connection string = EF contextname
                // grab the type name and we're done
                var configNameEf = string.IsNullOrEmpty(configConnectionStringName)
                    ? source.GetType().Name
                    : configConnectionStringName;

                // add a reference to System.Configuration
                var entityCnxStringBuilder = new EntityConnectionStringBuilder
                    (System.Configuration.ConfigurationManager
                        .ConnectionStrings[configNameEf].ConnectionString);

                // init the sqlbuilder with the full EF connectionstring cargo
                var sqlCnxStringBuilder = new SqlConnectionStringBuilder
                    (entityCnxStringBuilder.ProviderConnectionString);

                // only populate parameters with values if added
                if (!string.IsNullOrEmpty(initialCatalog))
                    sqlCnxStringBuilder.InitialCatalog = initialCatalog;
                if (!string.IsNullOrEmpty(dataSource))
                    sqlCnxStringBuilder.DataSource = dataSource;
                if (!string.IsNullOrEmpty(userId))
                    sqlCnxStringBuilder.UserID = userId;
                if (!string.IsNullOrEmpty(password))
                    sqlCnxStringBuilder.Password = password;

                // set the integrated security status
                sqlCnxStringBuilder.IntegratedSecurity = integratedSecuity;

                // now flip the properties that were changed
                source.Database.Connection.ConnectionString
                    = sqlCnxStringBuilder.ConnectionString;
            }
            catch (Exception ex)
            {
                // set log item if required
            }
        }
        public static void ChangeConnectionString(this DbContext source,
            string connectionString)
        {
            try
            {
                // add a reference to System.Configuration
                var entityCnxStringBuilder = new EntityConnectionStringBuilder(connectionString);
                // init the sqlbuilder with the full EF connectionstring cargo
                var sqlCnxStringBuilder = new SqlConnectionStringBuilder
                    (entityCnxStringBuilder.ProviderConnectionString);
                // now flip the properties that were changed
                source.Database.Connection.ConnectionString
                    = sqlCnxStringBuilder.ConnectionString;
            }
            catch (Exception ex)
            {
                // set log item if required
            }
        }
        public static string GetHash(string input)
        {
            using (HashAlgorithm hashAlgorithm = new SHA256CryptoServiceProvider())
            {
                var byteValue = Encoding.UTF8.GetBytes(input);
                var byteHash = hashAlgorithm.ComputeHash(byteValue);
                var result = Convert.ToBase64String(byteHash);
                char[] badChars = { '!', '@', '#', '$', '%', '_', '+', '=', '/', '\\' }; //simple example
                result = string.Concat(result.Split(badChars, StringSplitOptions.RemoveEmptyEntries));
                return result;
            }
        }
        public static string RandomString(int length=20)
        {
            // creating a StringBuilder object()
            StringBuilder str_build = new StringBuilder();
            Random random = new Random();

            char letter;

            for (int i = 0; i < length; i++)
            {
                double flt = random.NextDouble();
                int shift = Convert.ToInt32(Math.Floor(25 * flt));
                letter = Convert.ToChar(shift + 65);
                str_build.Append(letter);
            }
            return str_build.ToString();
        }
        public static T DataContractSerialization<T>(T obj)
        {
            DataContractSerializer dcSer = new DataContractSerializer(obj.GetType());
            MemoryStream memoryStream = new MemoryStream();

            dcSer.WriteObject(memoryStream, obj);
            memoryStream.Position = 0;

            T newObject = (T)dcSer.ReadObject(memoryStream);
            return newObject;
        }
        public static List<T> ToList<T>(this IEnumerator<T> e)
        {
            return ((IEnumerable<T>) e).ToList();
        }

        //public static FinanceStatus GetFinanceStatus()
        //{
        //    FinanceStatus status=FinanceStatus.NA;
        //    try
        //    {
        //        var context = (ClaimsPrincipal)HttpContext.Current.User;
        //        var claim = context?.Claims.FirstOrDefault(x => x.Type == "FinanceStatus");
        //        if (claim == null) return status;
        //        var obj = Convert.ChangeType(claim.Value, typeof(int));
        //        var raw= (int)obj;
        //        status = (FinanceStatus) raw;
        //    }
        //    catch (Exception)
        //    {//Ignore
        //    }

        //    return status;
        //}
        public static string UrlShortnerBaseAddress
        {
            get
            {
                return ConfigurationManager.AppSettings["UrlShortnerBaseAddress"]?.ToString();
            }
        }
        public static bool HostedOnPremise
        {
            get
            {
                bool.TryParse(ConfigurationManager.AppSettings["HostedOnPremise"], out var result);
                return result;
            }
        }
        public static bool EFTracingFlag
        {
            get
            {
                try
                {
                    bool.TryParse(ConfigurationManager.AppSettings["EFTracingFlag"], out var result);
                    return result;
                }
                catch {
                    return false;
                }
               
            }
        }
        public static string APICountryRegion
        {
            get
            {
                try
                {
                    
                    return ConfigurationManager.AppSettings["APICountryRegion"].ToString();
                }
                catch
                {
                    return "";
                }

            }
        }
        public static bool RedisCacheFlag
        {
            get
            {
                try
                {
                    bool.TryParse(ConfigurationManager.AppSettings["RedisCacheFlag"], out var result);
                    return result;
                }
                catch
                {
                    return false;
                }

            }
        }
        public static HangfireStorageType HanfireStorage
        {
            get
            {
                try
                {
                    Enum.TryParse(ConfigurationManager.AppSettings["HanfireStorage"]??ConfigurationManager.AppSettings["HangfireStorage"], out HangfireStorageType result);
                    return result;
                }
                catch
                {
                    return HangfireStorageType.redis;
                }

            }
        }
        public static string[] HangfireQueues
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["HangfireQueues"]?.Split('#')??new[]{"gofleet"};
                }
                catch
                {
                    return new[] {"gofleet"};
                }

            }
        }
        public static string RedisPassword
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["RedisPassword"];
                }
                catch
                {
                    return "YUib__(*)@#_($&lt;l__@#";
                }

            }
        }
        public static string RedisNetworkAddress
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["RedisNetworkAddress"];
                }
                catch
                {
                    return "127.0.0.1";
                }

            }
        }
        public static int RedisPort
        {
            get
            {
                try
                {
                    int.TryParse(ConfigurationManager.AppSettings["RedisPort"], out var result);
                    return result;
                }
                catch
                {
                    return 6379;
                }

            }
        }
        public static int RedisDatabase
        {
            get
            {
                try
                {
                    int.TryParse(ConfigurationManager.AppSettings["RedisDatabase"], out var result);
                    return result;
                }
                catch
                {
                    return 0;
                }

            }
        }

        public static string GatewayUrl
        {
            get
            {
                try
                {
                    var url = ConfigurationManager.AppSettings["GatewayUrl"];
                    return !string.IsNullOrWhiteSpace(url) ? url : "https://africa.iwlt.in";
                }
                catch
                {
                    return "https://africa.iwlt.in";
                }
               
            }
        }
        public static string WebAppUrl
        {
            get
            {
                try
                {
                    var url = ConfigurationManager.AppSettings["WebAppUrl"];
                    return !string.IsNullOrWhiteSpace(url) ? url : "https://app.iwlt.in";

                }
                catch
                {
                    return "https://app.iwlt.in";
                }
                
            }
        }
        public static string OnPremiseHostedConnectionString
        {
            get
            {
                var connection = ConfigurationManager.ConnectionStrings["HostedOnPremise"].ConnectionString;
                if (string.IsNullOrWhiteSpace(connection)) throw new BusinessException(ErrorCode.GLB103, "Hosted OnPremise configuration not found");
                return connection;
            }
        }
        public static string LoggedInTenantId
        {
            get
            {
                string status = string.Empty;
                try
                {
#if DEBUG
                    return "UNICORNTESTe328ae1644c98816bb8a";
#endif
                    var ctx = HttpContext.Current;
                    status = ctx?.GetOwinContext().Get<string>("as:tenantid") ?? "";
                    if (!string.IsNullOrWhiteSpace(status)) return status;
                    var context = (ClaimsPrincipal)ctx.User;
                    var claim = context?.Claims.FirstOrDefault(x => x.Type == "TenantId");
                    if (claim == null) return status;
                    return claim.Value;
                }
                catch (Exception)
                {//Ignore
                }

                return status;
            }
            
        }
        public static string ApiKey
        {
            get
            {
                string status = string.Empty;
                try
                {
                    var ctx = HttpContext.Current;
                    var context = (ClaimsPrincipal)ctx.User;
                    var claim = context?.Claims.FirstOrDefault(x => x.Type == "apikey");
                    if (claim != null)return claim.Value;
                    return ctx.Request.Headers.Get("apikey");
                }
                catch (Exception)
                {//Ignore
                }

                return status;
            }

        }
        public static string LoggedInTenantClientKey
        {
            get
            {
                string status = string.Empty;
                try
                {
                    var context = HttpContext.Current?.User as ClaimsPrincipal;
                    var claim = context?.Claims.FirstOrDefault(x => x.Type == "ClientKey");
                    if (claim == null) return status;
                    var obj = Convert.ChangeType(claim.Value, typeof(string));
                    var raw = (string)obj;
                    status = (string)raw;
                }
                catch (Exception)
                {//Ignore
                }

                return status;
            }

        }
        public static string UserName
        {
            get
            {
                string status = string.Empty;
                try
                {
                    var context = HttpContext.Current?.User as ClaimsPrincipal;
                    var claim = context?.Claims.FirstOrDefault(x => x.Type == "userName");
                    if (claim == null) return status;
                    var obj = Convert.ChangeType(claim.Value, typeof(string));
                    var raw = (string)obj;
                    status = (string)raw;
                }
                catch (Exception)
                {//Ignore
                }

                return status;
            }

        }
        public static string TenantEmailAddress
        {
            get
            {
                string status = string.Empty;
                try
                {
                    var context = HttpContext.Current?.User as ClaimsPrincipal;
                    var claim = context?.Claims.FirstOrDefault(x => x.Type == "TenantEmailAddress");
                    if (claim == null) return status;
                    var obj = Convert.ChangeType(claim.Value, typeof(string));
                    var raw = (string)obj;
                    status = (string)raw;
                }
                catch (Exception)
                {//Ignore
                }

                return status;

            }
        }
        public static string TenantName
        {
            get
            {
                string status = string.Empty;
                try
                {
                    var context = HttpContext.Current?.User as ClaimsPrincipal;
                    var claim = context?.Claims.FirstOrDefault(x => x.Type == "TenantName");
                    if (claim == null) return status;
                    var obj = Convert.ChangeType(claim.Value, typeof(string));
                    var raw = (string)obj;
                    status = (string)raw;
                }
                catch (Exception)
                {//Ignore
                }

                return status;
            }

        }
        public static string TenantShortName
        {
            get
            {
                string status = string.Empty;
                try
                {
                    var context = HttpContext.Current?.User as ClaimsPrincipal;
                    var claim = context?.Claims.FirstOrDefault(x => x.Type == "TenantShortName");
                    if (claim == null) return status;
                    var obj = Convert.ChangeType(claim.Value, typeof(string));
                    var raw = (string)obj;
                    status = (string)raw;
                }
                catch (Exception)
                {//Ignore
                }

                return status;
            }

        }
        public static string ApplicationId
        {
            get
            {
                string status = string.Empty;
                try
                {
                    var context = HttpContext.Current?.User as ClaimsPrincipal;
                    var claim = context?.Claims.FirstOrDefault(x => x.Type == "ApplicationId");
                    if (claim == null) return status;
                    var obj = Convert.ChangeType(claim.Value, typeof(string));
                    var raw = (string)obj;
                    status = (string)raw;
                }
                catch (Exception)
                {//Ignore
                }

                return status;
            }

        }

        public static long GetLoggedInUserId()
        {
            long status = 0;
            try
            {
                var ctx = HttpContext.Current;
                status = ctx?.GetOwinContext()?.Get<long>("userId")??0;
                if (status>0) return status;
                var context = HttpContext.Current?.User as ClaimsPrincipal;
                var claim = context?.Claims.FirstOrDefault(x => x.Type == "UserId");
                if (claim == null) return status;
                var obj = Convert.ChangeType(claim.Value, typeof(long));
                var raw = (long)obj;
                status = (long)raw;
            }
            catch (Exception)
            {//Ignore
            }

            return status;
        }
        public static string GetLoggedInUserFullName()
        {
            string status = string.Empty;
            try
            {
                var context = HttpContext.Current?.User as ClaimsPrincipal;
                var claim = context?.Claims.FirstOrDefault(x => x.Type == "UserFullName");
                if (claim == null) return status;
                var obj = Convert.ChangeType(claim.Value, typeof(string));
                var raw = (string)obj;

                status = (string)raw;
            }
            catch (Exception)
            {//Ignore
            }

            return status;
        }
        public static long SessionId()
        {
            long sessionId = 0;
            try
            {
                if (HttpContext.Current?.User == null) sessionId = 0;
                var ctx = HttpContext.Current?.User as ClaimsPrincipal;
                var sessionIdObj = ctx?.Claims.FirstOrDefault(x => x.Type == "SessionId");
                sessionId = long.Parse((sessionIdObj?.Value ?? "0"));
            }catch (Exception)
            {
                //Ignore
            }
            return sessionId;
        }
        public static int? ConstCurTypeId
        {
            get
            {
                int? _ConstCurTypeId = null;
                try
                {
                    if (HttpContext.Current?.User == null)
                    {
                        _ConstCurTypeId = null;
                    }
                    else
                    {
                        var ctx = HttpContext.Current?.User as ClaimsPrincipal;
                        var constCurTypeId = ctx?.Claims.FirstOrDefault(x => x.Type == "ConstCurTypeId") ?? null;
                        _ConstCurTypeId = int.Parse(constCurTypeId?.Value);
                    }
                }
                catch (Exception)
                {
                    _ConstCurTypeId = null;
                }
                return _ConstCurTypeId;
            }
        }
        private static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        public static DateTime FromUnixTime(this long unixTime)
        {
            return epoch.AddSeconds(unixTime);
        }
        public static long ToUnixTimeSeconds(this DateTime? date)
        {
            return (long)(date?.Subtract(epoch).TotalSeconds??0);
        }
        public static string CountryTimeZone
        {
            get
            {
                try
                {
                    return ConfigurationManager.AppSettings["CountryTimeZone"];
                }
                catch
                {
                    return "India Standard Time";
                }

            }
        }
    }
    public static class AppBuilderExtensions
    {
        public static void OnDisposing(this IAppBuilder app, Action cleanup)
        {
            var properties = new AppProperties(app.Properties);
            var token = properties.OnAppDisposing;
            if (token != CancellationToken.None)
            {
                token.Register(cleanup);
            }
        }
    }
    
    public enum HangfireStorageType
    {
        redis,
        mssql,
        sqlite
    }
}
