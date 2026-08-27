using EntityFramework.Caching;
using EntityFramework.Extensions;

using Newtonsoft.Json;

using Repository.Pattern.Core.Repositories;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.OData;

using TrackoApi.Core.Helpers;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;

using TrackoAPI.Infrastructure.Filters;
using TrackoAPI.Models.Shared;
using TrackoAPI.Reporting.Models;
using TrackoAPI.Reports.ViewModels.Global;
using TrackoAPI.WebUtilities.Helper;

namespace TrackoAPI.Reporting.Controller
{
    [AuthorizeEx]
    public class ReportsRequestPoolController : ODataController
    {
        private IRepositoryAsync<ReportProcedure> _procRepo;
        private IRepositoryAsync<ReportRequestPool> _service;
        public ReportsRequestPoolController(IRepositoryAsync<ReportRequestPool> repository, IRepositoryAsync<ReportProcedure> procRepo)
        {
            _service = repository;
            _procRepo = procRepo;
        }

        // DELETE: odata/Customers(5)
        public async Task<IHttpActionResult> Delete(long key)
        {
            var objReportParam = await _service.FindAsync(key);
            if (objReportParam == null)
            {
                return NotFound();
            }
            if (objReportParam.IsScheduled) return StatusCode(HttpStatusCode.Forbidden);
            objReportParam.ObjectState = ObjectState.Deleted;
            _service.Delete(objReportParam);
            await Request.GetContext().SaveChangesAsync();
            return StatusCode(HttpStatusCode.NoContent);
        }

        [HttpGet]
        public async Task<IHttpActionResult> DoesProcProvideJson([FromODataUri] long reportId, [FromODataUri] long procId)
        {
            var query = _procRepo.Queryable();
            if (reportId > 0) query = query.Where(x => x.ReportId == reportId);
            if (procId > 0) query = query.Where(x => x.Id == procId);
            var result = await query.Select(x => new { x.IsJson }).FromCacheFirstOrDefaultAsync(CachePolicy.WithDurationExpiration(TimeSpan.FromHours(5)));
            return Ok(result?.IsJson ?? false);
        }

        [HttpGet, EnableQuery]
        public IQueryable<ReportRequestPool> Get()
        {
            return _service.Queryable();
        }

        // GET: odata/ReportsRequestPool(5)
        [EnableQuery]
        public SingleResult<ReportRequestPool> Get([FromODataUri] long key)
        {
            return SingleResult.Create(_service.Queryable().Where(t => t.Id == key));
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetReport([FromODataUri]long key)
        {
            var uow = Request.GetContext();
            var startdate = DateTime.Now;
            var request = await _service.FindAsync(key);

            if (string.IsNullOrWhiteSpace(request?.Query))
            {
                return NotFound();
            }
            object[] parameters = { };

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
                    var result = await uow.SqlQueryAsync(request.Query, parameters);
                    request.Duration = DateTime.Now.Subtract(startdate).TotalSeconds;
                    await Request.GetContext().SaveChangesAsync();
                    return Ok(JsonConvert.SerializeObject(result));
                }
                else
                {
                    uow.BeginTransaction(IsolationLevel.ReadCommitted);
                    await uow.ExecuteProcedureAsync(request.Query, parameters);
                    request.Duration = DateTime.Now.Subtract(startdate).TotalSeconds;
                    await Request.GetContext().SaveChangesAsync();
                    uow.Commit();
                    return Ok();
                }
            }
            catch (SqlException ex)
            {
                if (request.IsCUD) uow.Rollback();
                //ex.ToExceptionless().AddObject(new { ReportRequestId = key }).Submit();
                throw new BusinessException(ErrorCode.GLB110, ex.Message);
            }
            catch (Exception ex)
            {
                if (request.IsCUD) uow.Rollback();
                //ex.ToExceptionless().AddObject(new { ReportRequestId = key }).Submit();
                throw new BusinessException(ErrorCode.GLB110, "Unable to Precess Report at Server");
            }
        }

        [HttpGet]
        public async Task<IHttpActionResult> GetReportV1([FromODataUri]long key)
        {
            var startdate = DateTime.Now;
            var request = await _service.FindAsync(key);

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
                    var result = await Request.GetContext().SqlQueryDataSetAsync(request.Query, parameters: parameters);
                    request.Duration = DateTime.Now.Subtract(startdate).TotalSeconds;
                    await Request.GetContext().SaveChangesAsync();
                    return Ok(JsonConvert.SerializeObject(result));
                }
                else
                {
                    var result = await Request.GetContext().ExecuteProcedureAsync(request.Query, parameters);
                    request.Duration = DateTime.Now.Subtract(startdate).TotalSeconds;
                    await Request.GetContext().SaveChangesAsync();
                    return Ok(JsonConvert.SerializeObject(result));
                }
            }
            catch (SqlException ex)
            {
                //ex.ToExceptionless().AddObject(new { ReportRequestId = key }).Submit();
                throw new BusinessException(ErrorCode.GLB110, ex.Message);
            }
            catch (Exception ex)
            {
                //ex.ToExceptionless().AddObject(new { ReportRequestId = key }).Submit();
                throw new BusinessException(ErrorCode.GLB110, "Unable to Precess Report at Server");
            }
        }

        [HttpPost]
        public async Task<IHttpActionResult> GetReportV2(ODataActionParameters ap)
        {
            if (!(ap["request"] is ReportRequestPool request))
            {
                return BadRequest("Parameter was null");
            }
            var uow = Request.GetContext();
            var st = new Stopwatch();
            st.Start();
            if (request.CustomReportId.GetValueOrDefault() == 0)
            {
                var procQuery = _procRepo.Queryable();
                if (request.PrintFormatDataSourceId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.PrintFormatDataSourceId == request.PrintFormatDataSourceId);
                }

                if (request.ProcId.GetValueOrDefault() == 0 && request.ReportId.GetValueOrDefault() == 0)
                {
                    return BadRequest("ProcId or ReportId is Required");
                }
                if (request.ProcId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.Id == request.ProcId);
                }

                if (request.ReportId > 0)
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
                await uow.ExecuteProcedureAsync($"[dbo].[Proc_GLB_UpdateReportCount]",
                    new SqlParameter("parameter1", SqlDbType.BigInt) { Value = proc.Id });
            }
            else
            {
                UserDefinedReportProcedure proc = await
                    uow
                        .RepositoryAsync<UserDefinedReportProcedure>()
                        .Queryable()
                        .Where(x => x.UserDefinedReportId == request.CustomReportId).FromCacheFirstOrDefaultAsync();
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("User Defined Report Not Configured.");
                }
                request.Query = proc.StoredProcedureName;
                request.ProcId = proc.Id;
                await uow.ExecuteProcedureAsync($"[dbo].[Proc_GLB_UpdateReportCount]",
                    new SqlParameter("parameter2", SqlDbType.BigInt) { Value = proc.Id });
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
                    var result = await uow.SqlQueryAsync(request.Query, parameters);
                    //await uow.SaveChangesAsync();
                    var json = JsonConvert.SerializeObject(result);
                    Request.Headers.TryAddWithoutValidation("ConsumedTime",
                        st.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

                    return Ok(json);
                }
                else
                {
                    var result = await uow.ExecuteProcedureAsync(request.Query, parameters);
                    //await uow.SaveChangesAsync();
                    Request.Headers.TryAddWithoutValidation("ConsumedTime",
                        st.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));

                    return Ok(JsonConvert.SerializeObject(result));
                }
                
            }
            catch (SqlException ex)
            {
                //ex.ToExceptionless().AddObject(request).Submit();
                throw new BusinessException(ErrorCode.GLB110, $"{ex.Message}, Entity:{request.Query}, Parameter:{GetParameterValues(parameters)}");
            }
            catch (Exception ex)
            {
                //ex.ToExceptionless().AddObject(request).Submit();
                throw new BusinessException(ErrorCode.GLB110, $"Unable to Precess Report at Server, Entity:{request.Query},  Parameter:{GetParameterValues(parameters)}");
            }
        }
        private string GetParameterValues(object[] parameters) {
            try
            {
                if (parameters != null)
                {
                    var paramStr = parameters.Select(p =>
                    {
                        if (p is SqlParameter sqlParam)
                            return $"{sqlParam.ParameterName}={sqlParam.Value ?? "NULL"}";
                        return p?.ToString() ?? "null";
                    });

                    return string.Join(", ", paramStr);
                }
                else
                {
                    return "";
                }
            }
            catch { return ""; }
        }

        //// PATCH: odata/ReportsRequestPool(5)
        /// PATCH performs a partial update. The client specifies just the properties to update.
        [AcceptVerbs("PATCH", "MERGE")]
        public async Task<IHttpActionResult> Patch([FromODataUri] long key, Delta<ReportRequestPool> patch)
        {
            return StatusCode(HttpStatusCode.Forbidden);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            ReportRequestPool objReportParam = await _service.FindAsync(key);
            if (objReportParam == null)
            {
                return NotFound();
            }
            objReportParam.ObjectState = ObjectState.Modified;
            patch.Patch(objReportParam);
            await Request.GetContext().SaveChangesAsync();
            return Updated(objReportParam);
        }

        // POST: odata/ReportsRequestPool
        public async Task<IHttpActionResult> Post(ReportRequestPool objReportParam)
        {
            objReportParam.ObjectState = ObjectState.Added;
            _service.Insert(objReportParam);

            if (objReportParam.CustomReportId.GetValueOrDefault() == 0)
            {
                var procQuery = _procRepo.Queryable();
                if (objReportParam.PrintFormatDataSourceId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.PrintFormatDataSourceId == objReportParam.PrintFormatDataSourceId);
                }
                if (objReportParam.ProcId.GetValueOrDefault() > 0)
                {
                    procQuery = procQuery.Where(x => x.Id == objReportParam.ProcId);
                }
                var proc = procQuery.FirstOrDefault(x => x.ReportId == objReportParam.ReportId);
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("Report Not Configured.");
                }
                objReportParam.Query = proc.StoredProcedureName;
                objReportParam.ProcId = proc.Id;
                objReportParam.IsCUD = proc.IsCUD;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }
            else
            {
                UserDefinedReportProcedure proc =
                    Request.GetContext()
                        .RepositoryAsync<UserDefinedReportProcedure>()
                        .Queryable()
                        .FirstOrDefault(x => x.UserDefinedReportId == objReportParam.CustomReportId);
                if (string.IsNullOrWhiteSpace(proc?.StoredProcedureName))
                {
                    return BadRequest("User Defined Report Not Configured.");
                }
                objReportParam.Query = proc.StoredProcedureName;
                objReportParam.CustomProcId = proc.Id;
                objReportParam.ReportId = null;
                objReportParam.ProcId = null;
                proc.UsaseCount++;
                proc.ObjectState = ObjectState.Modified;
            }

            objReportParam.ObjectState = ObjectState.Added;
            await Request.GetContext().SaveChangesAsync();
            return Created(objReportParam);
        }

        // PUT: odata/ReportsRequestPool(5)
        public async Task<IHttpActionResult> Put(long key, ReportRequestPool objReportParam)
        {
            return StatusCode(HttpStatusCode.Forbidden);
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            if (key != objReportParam.Id)
            {
                return BadRequest();
            }
            objReportParam.ObjectState = ObjectState.Modified;
            _service.Update(objReportParam);
            await Request.GetContext().SaveChangesAsync();

            return Updated(objReportParam);
        }
        [HttpGet]
        public IQueryable<vwReportSearch> SearchReport([FromODataUri] string searchTerm)
        {
            var reports = Request.GetContext().Context.ApiViews.AsNoTracking()
                .Where(x => x.DisplayText.Contains(searchTerm) || x.Id.ToString().Contains(searchTerm) || x.EntityType == AclType.Report)
                .Select(x => new vwReportSearch
                {
                    Id = x.Id,
                    IsUDR = "N",
                    ReportName = x.DisplayText + "[" + x.Id + "]"
                });
            var subreports = Request.GetContext().Context.UserDefinedReports.AsNoTracking()
                .Where(x => x.Name.Contains(searchTerm) || x.Id.ToString().Contains(searchTerm))
                .Select(x => new vwReportSearch
                {
                    Id = x.Id,
                    IsUDR = "Y",
                    ReportName = x.Name + "[" + x.Id + "]"
                });
            return reports.Union(subreports);
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
                if (!proc.ToLower().Contains($"@{field.Name.ToLower()}") || proc.ToLower().Contains($"@{field.Name.ToLower()}=") || proc.ToLower().Contains($"@{field.Name.ToLower()} =")) continue;
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