using EntityFramework.Extensions;

//using EntityFramework.Extensions;

using Newtonsoft.Json;
using Repository.Pattern.Core.UnitOfWork;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http;
using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Reporting.Models;

namespace TrackoAPI.Reporting.Controller
{
    [RoutePrefix("api/v2/Report"), AuthorizeEx]
    public class ReportController : ApiController
    {
        private IUnitOfWorkAsync _uow;

        public ReportController(IUnitOfWorkAsync unitOfWork)
        {
            _uow = unitOfWork;
        }

        [HttpPost, Route("DataSet")]
        public async Task<IHttpActionResult> GetDataSet([FromBody] ReportRequestPool request)
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
            request.CreatedSessionId = Helper.SessionId();
            request.CreatedDOE = DateTime.Now;
            if (request.CustomReportId.GetValueOrDefault() == 0)
            {
                var procQuery = _uow.RepositoryAsync<ReportProcedure>().Queryable();
                if (request.PrintFormatDataSourceId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.PrintFormatDataSourceId == request.PrintFormatDataSourceId);
                }
                if (request.ProcId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.Id == request.ProcId);
                }

                if (request.ReportId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.ReportId == request.ReportId);
                }
                var proc = await procQuery.FromCacheFirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("Report Not Configured.");
                }
                request.Query = proc.StoredProcedureName;
                request.ProcId = proc.Id;
                request.IsCUD = proc.IsCUD;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }
            else
            {
                UserDefinedReportProcedure proc =
                    _uow
                        .RepositoryAsync<UserDefinedReportProcedure>()
                        .Queryable()
                        .Where(x => x.UserDefinedReportId == request.CustomReportId)
                        .FromCacheFirstOrDefault();
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("User Defined Report Not Configured.");
                }
                request.Query = proc.StoredProcedureName;
                request.ProcId = proc.Id;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }

            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return NotFound();
            }
            object[] parameters = null;

            if (request.Query.Contains("@") && !request.Query.Contains("@reportrequestid"))
            {
                BuildCallable(request.Query, request, ref parameters);
            }
            if (request.Query.Contains("@reportrequestid"))
            {
                parameters = new object[] { new SqlParameter("reportrequestid", request.Id) };
            }
            try
            {
                request.IsExecuted = true;
                request.ObjectState = ObjectState.Modified;
                if (!request.IsCUD)
                {
                    //var result = await Request.GetContext().DynamicSqlQueryAsync(request.Query, parameters);
                    DataSet result = null;
                    if (string.IsNullOrWhiteSpace(request.Parameter30))
                    {
                        result = await _uow.SqlQueryDataSetAsync(request.Query, parameters: parameters);
                    }
                    else
                    {
                        var dictionary = JsonConvert.DeserializeObject<System.Collections.Generic.Dictionary<string, string>>(request.Parameter30);
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
                    var result = await _uow.ExecuteProcedureAsync(request.Query, parameters);
                    request.Duration = DateTime.Now.Subtract(startdate).TotalSeconds;
                    await _uow.SaveChangesAsync();
                    return Ok(JsonConvert.SerializeObject(result));
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

        [Route("ExcelReport"), HttpPost]
        public async Task<IHttpActionResult> GetExcelReport([FromBody] ReportRequestPool request)
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

            request.CreatedSessionId = Helper.SessionId();
            request.CreatedDOE = DateTime.Now;
            if (request.CustomReportId.GetValueOrDefault() == 0)
            {
                var procQuery = _uow.RepositoryAsync<ReportProcedure>().Queryable();
                if (request.PrintFormatDataSourceId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.PrintFormatDataSourceId == request.PrintFormatDataSourceId);
                }
                if (request.ProcId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.Id == request.ProcId);
                }
                var proc = await procQuery.Where(x => x.ReportId == request.ReportId).FromCacheFirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("Report Not Configured.");
                }
                request.Query = proc.StoredProcedureName;
                request.ProcId = proc.Id;
                request.IsCUD = proc.IsCUD;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }
            else
            {
                UserDefinedReportProcedure proc =
                    _uow
                        .RepositoryAsync<UserDefinedReportProcedure>()
                        .Queryable()
                        .Where(x => x.UserDefinedReportId == request.CustomReportId)
                        .FromCacheFirstOrDefault();
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("User Defined Report Not Configured.");
                }
                request.Query = proc.StoredProcedureName;
                request.ProcId = proc.Id;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }

            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return NotFound();
            }
            object[] parameters = null;

            if (request.Query.Contains("@"))
            {
                BuildCallable(request.Query, request, ref parameters);
            }

            try
            {
                if (!request.IsCUD)
                {
                    //var result = await Request.GetContext().DynamicSqlQueryAsync(request.Query, parameters);
                    var result = await _uow.SqlQueryAsync(request.Query, parameters);
                    await _uow.SaveChangesAsync();
                    //var json = JsonConvert.SerializeObject(result);
                    return Ok(result);
                }
                else
                {
                    var result = await _uow.ExecSqlQueryAsync(request.Query, parameters);
                    await _uow.SaveChangesAsync();
                    return Ok(result);
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

        [Route("GetJSONReport"), HttpPost]
        public async Task<IHttpActionResult> GetJSONReport([FromBody] ReportRequestPool request)
        {
            bool isJSONProc = false;
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (request == null)
            {
                return BadRequest("Parameter was null");
            }
            request.CreatedSessionId = Helper.SessionId();
            request.CreatedDOE = DateTime.Now;
            if (request.CustomReportId.GetValueOrDefault() == 0)
            {
                var procQuery = _uow.RepositoryAsync<ReportProcedure>().Queryable();
                if (request.PrintFormatDataSourceId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.PrintFormatDataSourceId == request.PrintFormatDataSourceId);
                }
                if (request.ProcId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.Id == request.ProcId);
                }
                var proc = await procQuery.Where(x => x.ReportId == request.ReportId).FromCacheFirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("Report Not Configured.");
                }
                isJSONProc = proc.IsJson;
                request.Query = proc.StoredProcedureName;
                request.ProcId = proc.Id;
                request.IsCUD = proc.IsCUD;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }
            else
            {
                UserDefinedReportProcedure proc =
                    _uow
                        .RepositoryAsync<UserDefinedReportProcedure>()
                        .Queryable()
                        .Where(x => x.UserDefinedReportId == request.CustomReportId)
                        .FromCacheFirstOrDefault();
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("User Defined Report Not Configured.");
                }
                request.Query = proc.StoredProcedureName;
                request.ProcId = proc.Id;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }

            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return NotFound();
            }
            object[] parameters = null;

            if (request.Query.Contains("@"))
            {
                BuildCallable(request.Query, request, ref parameters);
            }

            try
            {
                if (!request.IsCUD)
                {
                    if (!isJSONProc)
                    {
                        var resultdt = await _uow.SqlQueryAsync(request.Query, parameters);
                        await _uow.SaveChangesAsync();
                        return Ok(resultdt);
                    }
                    var resultjson = await _uow.SqlQueryAsJsonAsync(request.Query, parameters);
                    await _uow.SaveChangesAsync();
                    return Ok(JsonConvert.DeserializeObject(resultjson));
                }
                var result = await _uow.ExecSqlQueryAsync(request.Query, parameters);
                await _uow.SaveChangesAsync();
                return Ok(JsonConvert.SerializeObject(result));
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

        [Route("GetReport"), HttpPost]
        public async Task<IHttpActionResult> GetReport([FromBody] ReportRequestPool request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (request == null)
            {
                return BadRequest("Parameter was null");
            }

            request.CreatedSessionId = Helper.SessionId();
            request.CreatedDOE = DateTime.Now;
            if (request.CustomReportId.GetValueOrDefault() == 0)
            {
                var procQuery = _uow.RepositoryAsync<ReportProcedure>().Queryable();
                if (request.PrintFormatDataSourceId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.PrintFormatDataSourceId == request.PrintFormatDataSourceId);
                }
                if (request.ProcId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.Id == request.ProcId);
                }
                var proc = await procQuery.Where(x => x.ReportId == request.ReportId).FromCacheFirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("Report Not Configured.");
                }
                request.Query = proc.StoredProcedureName;
                request.ProcId = proc.Id;
                request.IsCUD = proc.IsCUD;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }
            else
            {
                UserDefinedReportProcedure proc =
                    _uow
                        .RepositoryAsync<UserDefinedReportProcedure>()
                        .Queryable()
                        .Where(x => x.UserDefinedReportId == request.CustomReportId)
                        .FromCacheFirstOrDefault();
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("User Defined Report Not Configured.");
                }
                request.Query = proc.StoredProcedureName;
                request.ProcId = proc.Id;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }

            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return NotFound();
            }
            object[] parameters = null;

            if (request.Query.Contains("@"))
            {
                BuildCallable(request.Query, request, ref parameters);
            }

            try
            {
                if (!request.IsCUD)
                {
                    //var result = await Request.GetContext().DynamicSqlQueryAsync(request.Query, parameters);
                    var result = await _uow.SqlQueryAsync(request.Query, parameters);
                    await _uow.SaveChangesAsync();
                    //var json = JsonConvert.SerializeObject(result);
                    return Ok(result);
                }
                else
                {
                    var result = await _uow.ExecSqlQueryAsync(request.Query, parameters);
                    await _uow.SaveChangesAsync();
                    return Ok(JsonConvert.SerializeObject(result));
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

        [Route("GetStationary"), HttpGet]
        public async Task<IHttpActionResult> GetStationary([FromUri] long fieldId, [FromUri] long officeId,
            [FromUri] long partyId, [FromUri] long viewId, [FromUri] long extraTypeId, [FromUri] DateTime date, [FromUri]string searchTerm = "", [FromUri] string data1 = "", [FromUri] string data2 = "", [FromUri] string data3 = "", [FromUri] string data4="")
        {
            var parameters = new List<SqlParameter>()
            {
                new SqlParameter("parameter1", fieldId),
                new SqlParameter("parameter2", officeId),
                new SqlParameter("parameter3", partyId),
                new SqlParameter("parameter4", viewId),
                new SqlParameter("parameter5", extraTypeId),
                new SqlParameter("parameter6", date),
                new SqlParameter("parameter7", searchTerm.Replace(" ","%"))                
            };
            if (!string.IsNullOrWhiteSpace(data1))
            {
                parameters.Add(new SqlParameter("parameter8", data1));
            }
            if (!string.IsNullOrWhiteSpace(data2))
            {
                parameters.Add(new SqlParameter("parameter9", data2));
            }
            if (!string.IsNullOrWhiteSpace(data3))
            {
                parameters.Add(new SqlParameter("parameter10", data3));
            }
            if (!string.IsNullOrWhiteSpace(data4))
            {
                parameters.Add(new SqlParameter("parameter11", data4));
            }
            var result = await _uow.SqlQueryAsync("[dbo].[Proc_GLB_GetStationary]", parameters.ToArray());
            return Ok(result);
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