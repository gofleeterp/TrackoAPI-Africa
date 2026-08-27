using Dapper;

using Hangfire.Console;
using Hangfire.Server;

using Newtonsoft.Json.Linq;

using RestSharp;

using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity.Core.EntityClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using TrackoApi.Core;
using TrackoApi.Data;
using TrackoApi.Models.Global;

namespace TrackoApi.Service.Global
{
    
    public  class EWayBillService
    {
        private readonly IGlobalStore _gs;
        public EWayBillService(IGlobalStore globalStore)
        {
            _gs = globalStore;
        }

        public void ExtendEWayBills(string tenantId,string gstin, PerformContext context = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(tenantId))
                {
                    return;
                }
                var authToken = "";
                var serverurl = "";
                var username = "";
                var password = "";
                using (var conn = _gs.CreateDbConnection(tenantId))
                {
                    if (conn.State == ConnectionState.Open) conn.Open();
                    using (var cmd = conn.CreateCommand())
                    {
                        cmd.CommandText = $"select top 1 ServerUrl=[dbo].[ENV_Server]('EWB_URL'),UserName=[dbo].[ENV_Server]('EWB_USERNAME__{gstin}'),Password=[dbo].[ENV_Server]('EWB_PASSWORD_{gstin}'),Token=JSON_VALUE(hrp.Result,'$.access_token') from tHttpRequestPool hrp where hrp.Result IS NOT NULL AND Purpose = 'adaequare_token_renew' ORDER BY hrp.CreatedTime desc";
                        cmd.CommandType = CommandType.Text;
                        var res= cmd.ExecuteReader();
                        while (res.Read())
                        {
                            serverurl=res.GetString(0);
                            username = res.GetString(1);
                            password = res.GetString(2);
                            authToken = res.GetString(3);
                        }
                    }
                    if (string.IsNullOrWhiteSpace(authToken)|| string.IsNullOrWhiteSpace(serverurl) || string.IsNullOrWhiteSpace(username)|| string.IsNullOrWhiteSpace(password))
                    {
                        context.WriteLine(color: ConsoleTextColor.Red, "One of required parameter was missing");
                        context.WriteLine(color: ConsoleTextColor.Red, new
                        {
                            authToken,
                            serverurl,
                            username,
                            password
                        });
                        return;
                    }
                    var ewaybills = conn.Query<eway_update>("", new { gstin});

                }

            }
            catch (Exception ex)
            {
                context.WriteLine(ConsoleTextColor.Red, ex);
            }
        }
        private async Task<IRestResponse> Adaequare_ExtendEWayBillByNo(IRestClient client,string tenantId,string token,string gstinNo, eway_update requestData,string userName,string password,long logId,string ewabillno,string batchId)
        {
            IRestResponse res = null;
            
            using (var conn = _gs.CreateDbConnection(tenantId))
            {
                try
                {
                    var reqid = Guid.NewGuid().ToString("D");
                    #region HttpRequest                  
                    var request = new RestRequest("ewayapi?action=EXTENDVALIDITY", Method.POST, DataFormat.Json);
                    request.AddHeader("username", userName);
                    request.AddHeader("password", password);
                    request.AddHeader("gstin", gstinNo);
                    request.AddHeader("requestid", reqid);
                    request.AddHeader("Authorization", token);
                    request.AddJsonBody(requestData);
                    res = await client.ExecuteTaskAsync(request);
                    #endregion
                    #region Log HttpRequest   
                    try
                    {
                        if (conn.State != ConnectionState.Open) conn.Open();
                        using (var cmd = conn.CreateCommand())
                        {
                            cmd.CommandText = "Proc_HttpReqPool_Insert";
                            cmd.CommandType = CommandType.StoredProcedure;
                            var p_reqid = cmd.CreateParameter();
                            p_reqid.ParameterName = "RequestId";
                            p_reqid.Value = reqid;
                            p_reqid.DbType = DbType.String;
                            p_reqid.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_reqid);


                            var p_method = cmd.CreateParameter();
                            p_method.ParameterName = "Method";
                            p_method.Value = "POST";
                            p_method.DbType = DbType.String;
                            p_method.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_method);

                            var p_uri = cmd.CreateParameter();
                            p_uri.ParameterName = "Uri";
                            p_uri.Value = request.Resource;
                            p_uri.DbType = DbType.String;
                            p_uri.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_uri);


                            var p_reqBody = cmd.CreateParameter();
                            p_reqBody.ParameterName = "RequestBody";
                            p_reqBody.Value = SimpleJson.SerializeObject(requestData);
                            p_reqBody.DbType = DbType.String;
                            p_reqBody.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_reqBody);


                            var p_header = cmd.CreateParameter();
                            p_header.ParameterName = "Headers";
                            p_header.Value = SimpleJson.SerializeObject(request.Parameters.Where(x => x.Type == ParameterType.HttpHeader).Select(parameter => new { name = parameter.Name, value = parameter.Value }));
                            p_header.DbType = DbType.String;
                            p_header.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_header);

                            var caller = "TrackoApi.Service.Global.EWayBillService.Adaequare_ExtendEWayBillByNo";
                            var p_sender = cmd.CreateParameter();
                            p_sender.ParameterName = "Sender";
                            p_sender.Value = caller;
                            p_sender.DbType = DbType.String;
                            p_sender.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_sender);

                            var p_purpose = cmd.CreateParameter();
                            p_purpose.ParameterName = "Purpose";
                            p_purpose.Value = "adaequare_postewb_extend";
                            p_purpose.DbType = DbType.String;
                            p_purpose.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_purpose);


                            var p_batchId = cmd.CreateParameter();
                            p_batchId.ParameterName = "BatchId";
                            p_batchId.Value = batchId;
                            p_batchId.DbType = DbType.String;
                            p_batchId.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_batchId);

                            var datenow = DateTime.Now;
                            var p_createdTime = cmd.CreateParameter();
                            p_createdTime.ParameterName = "CreatedTime";
                            p_createdTime.Value = datenow;
                            p_createdTime.DbType = DbType.DateTime;
                            p_createdTime.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_createdTime);

                            var p_processTime = cmd.CreateParameter();
                            p_processTime.ParameterName = "ProcessTime";
                            p_processTime.Value = datenow;
                            p_processTime.DbType = DbType.DateTime;
                            p_processTime.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_processTime);

                            var p_executedTime = cmd.CreateParameter();
                            p_executedTime.ParameterName = "ExecutedTime";
                            p_executedTime.Value = datenow;
                            p_executedTime.DbType = DbType.DateTime;
                            p_executedTime.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_executedTime);


                            var p_timeout = cmd.CreateParameter();
                            p_timeout.ParameterName = "Timeout";
                            p_timeout.Value = 6000;
                            p_timeout.DbType = DbType.Int16;
                            p_timeout.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_timeout);

                            var p_result = cmd.CreateParameter();
                            p_result.ParameterName = "Result";
                            if (!string.IsNullOrWhiteSpace(res.ErrorMessage))
                            {
                                p_result.Value = res.ErrorMessage;
                            }
                            if (!string.IsNullOrWhiteSpace(res.Content))
                            {
                                p_result.Value = res.Content;
                            }
                            if (!string.IsNullOrWhiteSpace(res.ErrorException?.GetBaseException().Message))
                            {
                                p_result.Value = res.ErrorException?.GetBaseException().Message;
                            }
                            p_result.DbType = DbType.String;
                            p_result.Direction = ParameterDirection.Input;
                            cmd.Parameters.Add(p_result);
                            cmd.ExecuteNonQuery();
                        }
                    }catch(Exception ex)
                    {
                        //Log it later
                    }
                    finally
                    {
                        conn.Close();
                    }
                   
                    #endregion
                    if (res.IsSuccessful)
                    {
                        try
                        {
                            if (conn.State != ConnectionState.Open) conn.Open();
                            using (var cmd = conn.CreateCommand())
                            {
                                cmd.CommandText = "Proc_EWB_ExtendExpiryDate";
                                cmd.CommandType = CommandType.StoredProcedure;

                                var p_result = cmd.CreateParameter();
                                p_result.ParameterName = "Result";
                                p_result.Value = res.Content;
                                p_result.DbType = DbType.String;
                                p_result.Direction = ParameterDirection.Input;
                                cmd.Parameters.Add(p_result);

                                var p_api = cmd.CreateParameter();
                                p_api.ParameterName = "API";
                                p_api.Value = "Adaequare";
                                p_api.DbType = DbType.String;
                                p_api.Direction = ParameterDirection.Input;
                                cmd.Parameters.Add(p_api);

                                var p_ewbno = cmd.CreateParameter();
                                p_ewbno.ParameterName = "EWBNo";
                                p_ewbno.Value = ewabillno;
                                p_ewbno.DbType = DbType.String;
                                p_ewbno.Direction = ParameterDirection.Input;
                                cmd.Parameters.Add(p_ewbno);

                                var p_logid = cmd.CreateParameter();
                                p_logid.ParameterName = "LogId";
                                p_logid.Value = ewabillno;
                                p_logid.DbType = DbType.Int64;
                                p_logid.Direction = ParameterDirection.Input;
                                cmd.Parameters.Add(p_logid);

                                cmd.ExecuteNonQuery();
                            }
                        }
                        catch (Exception ex)
                        {
                            //Log it later
                        }
                        finally
                        {
                            conn.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    //Log it later
                }
                if (conn.State == ConnectionState.Open)
                {
                    conn.Close();
                }
            }
            return res;
        }
    }
    public class eway_update
    {
        public long ewbNo { get; set; }
        public string vehicleNo { get; set; }
        public string fromPlace { get; set; }
        public string fromState { get; set; }
        public string remainingDistance { get; set; }
        public string transDocNo { get; set; }
        public string transDocDate { get; set; }
        public string transMode { get; set; }
        public string fromPincode { get; set; }
        public string consignmentStatus { get; set; }
        public string extnRsnCode { get; set; }
        public string extnRemarks { get; set; }
        public string transitType { get; set; }
    }
    
}
