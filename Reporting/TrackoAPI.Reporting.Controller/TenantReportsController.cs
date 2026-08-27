using EntityFramework.Extensions;

//using EntityFramework.Extensions;

using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;

using Tenant.Models;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Reporting.Models;

namespace TrackoAPI.Reporting.Controller
{
    [RoutePrefix("tenant/api/Reports"), AuthorizeEx]
    public class TenantReportsController : ApiController
    {
        private ITenantDbContext _uow;

        public TenantReportsController(ITenantDbContext db)
        {
            _uow = db;
        }

        [HttpPost, Route("DataSet")]
        public async Task<IHttpActionResult> GetDataSet([FromBody] TenantReportRequestPool request)
        {
            var startdate = DateTime.Now;

            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (request == null)
            {
                return BadRequest("Parameter was null");
            }
            request.CSID = Helper.SessionId();
            request.CDOE = DateTime.Now;
            var proc = await _uow.ReportProcedure.FindAsync(request.ProcId);
            if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
            {
                return BadRequest("Report Not Configured.");
            }
            request.Query = proc.StoredProcedureName;
            request.ProcId = proc.Id;
            request.IsCUD = proc.IsCUD;
            proc.UsageCount++;
            proc.ObjectState = ObjectState.Modified;

            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return NotFound();
            }
            object[] parameters = null;
            var paramlabels = request.Query?.Split('@').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (paramlabels.Length > 1)
            {
                if (paramlabels.Any(x => x == "id"))
                {
                    parameters = new object[] { new SqlParameter("id", request.Id) };
                    _uow.ReportRequestPool.Add(request);
                    await _uow.SaveChangesAsync();
                }
                else
                {
                    if (proc.MultipleParams)
                    {
                        System.Collections.Generic.Dictionary<string, object> @params = null;
                        try
                        {
                            @params = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(request.JsonProps);
                        }
                        catch
                        {
                            return BadRequest("JsonData should of type KeyValue Pair e.g. {\"first_name\":\"arvind\",\"last_name\":\"singh\"}");
                        }
                        parameters = @params.Select(x => new SqlParameter(x.Key, x.Value)).ToArray();
                    }
                    else
                    {
                        parameters = new object[] { new SqlParameter(paramlabels.LastOrDefault(), request.JsonProps) };
                    }
                }
            }
            try
            {
                request.IsExecuted = true;
                request.ObjectState = ObjectState.Modified;
                
                if (!request.IsCUD)
                {
                    //var result = await Request.GetContext().DynamicSqlQueryAsync(request.Query, parameters);
                    DataSet result = null;
                    if (string.IsNullOrWhiteSpace(request.TableNameMapping))
                    {
                        result = await _uow.SqlQueryDataSetAsync(request.Query, parameters: parameters);
                    }
                    else
                    {
                        var dictionary = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(request.TableNameMapping);
                        result = await _uow.SqlQueryDataSetAsync(request.Query, dictionary, parameters);
                    }

                    request.Duration = DateTime.Now.Subtract(startdate).TotalSeconds;
                    await _uow.SaveChangesAsync();
                    //DataTable[] array = { };
                    //result.Tables.CopyTo(array,0);
                    return Json(result, new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented
                    });
                }
                else
                {
                    await _uow.ExecuteProcedureAsync(request.Query, parameters);
                    request.Duration = DateTime.Now.Subtract(startdate).TotalSeconds;
                    await _uow.SaveChangesAsync();
                    return Ok();
                }
            }
            catch (SqlException ex)
            {
                throw new BusinessException(ErrorCode.GLB110, ex.Message);
            }
            catch (Exception ex)
            {
                throw new BusinessException(ErrorCode.GLB110, "Unable to Precess Report at Server");
            }
        }

        [Route("Excel"), HttpPost]
        public async Task<IHttpActionResult> GetExcelReport([FromBody] TenantReportRequestPool request)
        {
            Request.Headers.Accept.Clear();
            Request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"));
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (request == null)
            {
                return BadRequest("Parameter was null");
            }

            request.CSID = Helper.SessionId();
            request.CDOE = DateTime.Now;
            var proc = await _uow.ReportProcedure.FindAsync(request.ProcId);
            if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
            {
                return BadRequest("Report Not Configured.");
            }
            request.Query = proc.StoredProcedureName;
            request.ProcId = proc.Id;
            request.IsCUD = proc.IsCUD;
            proc.UsageCount++;
            proc.ObjectState = ObjectState.Modified;

            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return NotFound();
            }
            object[] parameters = null;

            var paramlabels = request.Query?.Split('@').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (paramlabels.Length > 1)
            {
                if (paramlabels.Any(x => x == "id"))
                {
                    parameters = new object[] { new SqlParameter("id", request.Id) };
                    _uow.ReportRequestPool.Add(request);
                    await _uow.SaveChangesAsync();
                }
                else
                {
                    if (proc.MultipleParams)
                    {
                        System.Collections.Generic.Dictionary<string, object> @params = null;
                        try
                        {
                            @params = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(request.JsonProps);
                        }
                        catch
                        {
                            return BadRequest("JsonData should of type KeyValue Pair e.g. {\"first_name\":\"arvind\",\"last_name\":\"singh\"}");
                        }
                        parameters = @params.Select(x => new SqlParameter(x.Key, x.Value)).ToArray();
                    }
                    else
                    {
                        parameters = new object[] { new SqlParameter(paramlabels.LastOrDefault(), request.JsonProps) };
                    }
                }
            }

            try
            {
                var result = await _uow.SqlQueryAsync(request.Query, parameters);
                await _uow.SaveChangesAsync();
                return Ok(result);
            }
            catch (SqlException ex)
            {
                throw new BusinessException(ErrorCode.GLB110, ex.Message);
            }
            catch (Exception ex)
            {
                throw new BusinessException(ErrorCode.GLB110, "Unable to Precess Report at Server");
            }
        }


        [Route("Execute"), HttpPost]
        public async Task<IHttpActionResult> GetReport([FromBody] TenantReportRequestPool request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (request == null)
            {
                return BadRequest("Parameter was null");
            }

            request.CSID = Helper.SessionId();
            request.CDOE = DateTime.Now;
            var proc = await _uow.ReportProcedure.FindAsync(request.ProcId);
            if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
            {
                return BadRequest("Report Not Configured.");
            }
            request.Query = proc.StoredProcedureName;
            request.ProcId = proc.Id;
            request.IsCUD = proc.IsCUD;
            proc.UsageCount++;
            proc.ObjectState = ObjectState.Modified;

            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return NotFound();
            }
            object[] parameters = null;

            var paramlabels = request.Query?.Split('@').Select(x => x.Trim()).Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();
            if (paramlabels.Length > 1)
            {
                if (paramlabels.Any(x => x == "id"))
                {
                    parameters = new object[] { new SqlParameter("id", request.Id) };
                    _uow.ReportRequestPool.Add(request);
                    await _uow.SaveChangesAsync();
                }
                else
                {
                    if (proc.MultipleParams)
                    {
                        System.Collections.Generic.Dictionary<string, object> @params = null;
                        try
                        {
                            @params = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, object>>(request.JsonProps);
                        }
                        catch
                        {
                            return BadRequest("JsonData should of type KeyValue Pair e.g. {\"first_name\":\"arvind\",\"last_name\":\"singh\"}");
                        }
                        parameters = @params.Select(x => new SqlParameter(x.Key, x.Value)).ToArray();
                    }
                    else
                    {
                        parameters = new object[] { new SqlParameter(paramlabels.LastOrDefault(), request.JsonProps) };
                    }
                }
            }

            try
            {
                if (!request.IsCUD)
                {
                    //var result = await Request.GetContext().DynamicSqlQueryAsync(request.Query, parameters);
                    var result = await _uow.SqlQueryAsync(request.Query, parameters);
                    await _uow.SaveChangesAsync();
                    //var json = JsonConvert.SerializeObject(result);
                    //return Ok(result);
                    return Json(result, new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        ContractResolver=new DefaultContractResolver()
                    });
                }
                else
                {
                    await _uow.ExecuteProcedureAsync(request.Query, parameters);
                    await _uow.SaveChangesAsync();
                    return Ok();
                }
            }
            catch (SqlException ex)
            {
                throw new BusinessException(ErrorCode.GLB110, ex.Message);
            }
            catch (Exception ex)
            {
                throw new BusinessException(ErrorCode.GLB110, "Unable to Precess Report at Server");
            }
        }

        private static PropertyInfo[] GetProperties(object obj)
        {
            return obj.GetType().GetProperties();
        }

        private void BuildCallable(string proc, ReportRequestPool req, ref object[] parameters)
        {
            req.CreatedSessionId = Helper.SessionId();
            req.CreatedDOE = DateTime.Now;
            var fields = GetProperties(req);
            var list = new List<object>();

            foreach (var field in fields)
            {
                if (!proc.ToLower().Contains($"@{field.Name.ToLower()}") || proc.ToLower().Contains($"@{field.Name.ToLower()}=")) continue;
                if (parameters == null)
                {
                    parameters = new object[] { };
                }

                var value = field.GetValue(req, null)?.ToString();
                list.Add(string.IsNullOrWhiteSpace(value)
                    ? new SqlParameter(field.Name.ToLower(), DBNull.Value)
                    : new SqlParameter(field.Name.ToLower(), value));
            }
            if (list.Any())
            {
                parameters = list.ToArray();
            }
        }
    }
}