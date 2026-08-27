using Microsoft.OData.Core;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity.Infrastructure;
using System.Data.Entity.Validation;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using TrackoApi.Core.Properties;

namespace TrackoApi.Core.Helpers
{
    public enum ErrorCode
    {
        /// <summary>
        /// The Document date does not exists between any financial year.
        /// </summary>
        VCH100,

        /// <summary>
        /// Cannot Modify Accepted Transaction
        /// </summary>
        VCH101,

        /// <summary>
        /// The Audited Voucher Transaction cannot be deleted.
        /// </summary>
        VCH102,

        /// <summary>
        /// Used Reference cannot be deleted
        /// </summary>
        VCH103,

        /// <summary>
        /// Validation failed for Amount.
        /// </summary>
        TADV100,

        /// <summary>
        /// Atleast two VD are required in Advance Transaction Voucher
        /// </summary>
        TADV101,

        /// <summary>
        /// Atlead one VDR is Required in Advance Transaction
        /// </summary>
        TADV102,

        /// <summary>
        /// VD and VDR Amount Doesn't Tally
        /// </summary>
        TADV103,

        /// <summary>
        /// Device By Zero Exception
        /// </summary>
        TADV104,

        /// <summary>
        /// Token is Expired
        /// </summary>
        GLB100,

        /// <summary>
        /// Authentication Failed
        /// </summary>
        GLB102,

        /// <summary>
        /// Credit and Debit Amount mismatch for Voucher
        /// </summary>
        VCH104,

        /// <summary>
        /// Atleast two Voucher Details are required in Voucher
        /// </summary>
        VCH105,

        /// <summary>
        /// VoucherDetail and VoucherDetailReference Amount Doesn't Tally
        /// </summary>
        VCH106,

        /// <summary>
        /// Cannot Delete Settled Trip Advance
        /// </summary>
        TADV105,

        /// <summary>
        /// Unable to Delete Settled TripLog
        /// </summary>
        TAL100,

        /// <summary>
        /// Bad Data in Attached Advances
        /// </summary>
        TS100,

        /// <summary>
        /// Bad Data in Attached Trip Expanses
        /// </summary>
        TS101,

        /// <summary>
        /// Bad Data in Attached Vehicle Movement logs(TripLogs)
        /// </summary>
        TS102,

        /// <summary>
        /// Bad Data in Attached Fuel Expanses
        /// </summary>
        TS103,

        /// <summary>
        /// The Vdr Reference not Found
        /// </summary>
        VCH107,

        /// <summary>
        /// Invalid Operation of Trip Expanse Entity
        /// </summary>
        TEXP100,

        /// <summary>
        /// Unable to generate prepaid tax entry
        /// </summary>
        DUET100,

        /// <summary>
        /// Invalid VoucherId
        /// </summary>
        VCH108,

        /// <summary>
        /// Insufficient amount available for against reference entry
        /// </summary>
        VCH109,

        /// <summary>
        /// Expiry date should be greater than or equal to Start Date
        /// </summary>
        DUET101,

        /// <summary>
        /// Start Date should be greater than or equal to Paid Date and Paid Date should be in same Financial Year.
        /// </summary>
        DUET102,

        /// <summary>
        /// PrePaid Tax Transaction entry already exists
        /// </summary>
        DUET103,

        /// <summary>
        /// Configuration not found for specified key
        /// </summary>
        GLB103,

        /// <summary>
        /// Data Conflict Occurred
        /// </summary>
        GLB104,

        /// <summary>
        /// Cannot Modify Referenced Transaction
        /// </summary>
        GLB105,

        /// <summary>
        /// Invalid Ledger provided
        /// </summary>
        VCH110,

        /// <summary>
        /// Atleast One VoucherDetailReference is required.
        /// </summary>
        VCH111,

        /// <summary>
        /// Bad Data in Spare Details of Transaction
        /// </summary>
        SPB100,

        /// <summary>
        /// Bad Data in Labour Details of Transaction
        /// </summary>
        SPB101,

        /// <summary>
        /// Validation Failed
        /// </summary>
        GLB106,

        /// <summary>
        /// Insufficient Balance Qty
        /// </summary>
        SPB102,

        /// <summary>
        /// Issued/Transfered Item cannot be deleted
        /// </summary>
        SPB103,

        /// <summary>
        /// Request Rejected
        /// </summary>
        GLB107,

        /// <summary>
        /// Used Transaction cannot be deleted
        /// </summary>
        GLB108,

        /// <summary>
        /// Invalid Tyre Id
        /// </summary>
        TYR100,

        /// <summary>
        /// Invalid Tyre Parent Identifier
        /// </summary>
        TYR101,

        /// <summary>
        /// Invalid Tyre Number
        /// </summary>
        TYR102,

        /// <summary>
        /// Tyre Transaction Date is invalid
        /// </summary>
        TYR103,

        /// <summary>
        /// Cannot Save Chassis Tyres where Vehicle Owner is not defined
        /// </summary>
        VEH100,

        /// <summary>
        /// Not Found
        /// </summary>
        GLB109,

        /// <summary>
        /// Financial year is Locked
        /// </summary>
        VCH112,

        /// <summary>
        /// Voucher Amount cannot be Zero
        /// </summary>
        VCH113,

        /// <summary>
        /// Unable to Execute Report Procedure
        /// </summary>
        GLB110,

        /// <summary>
        /// Internal Configuration Update Not Allowed
        /// </summary>
        GLB111,

        /// <summary>
        /// Transaction has been Approved: Hint FormId 1753
        /// </summary>
        GLB112,

        /// <summary>
        /// Trip Advance has already been settled in another Settlement
        /// </summary>
        TADV106,

        /// <summary>
        /// The Vehicle in Trip Advance and Vehicle in mapped Trip should be same.
        /// </summary>
        TADV107,
        /// <summary>
        /// The Vehicle in Trip Advance has been Disbursed and cannot be updated or deleted.
        /// </summary>
        TADV108,

        /// <summary>
        /// Background Job Failed
        /// </summary>
        JobFailed,

        /// <summary>
        /// Event Failed.
        /// </summary>
        EventFailed,

        /// <summary>
        /// Cannot update/Insert this transaction as given dates overlap with other transaction
        /// </summary>
        TAL101,

        /// <summary>
        /// Account has been locked
        /// </summary>
        VCH114,
        /// <summary>
        /// Currency Rate is required
        /// </summary>
        CUR100,
        /// <summary>
        /// Forex Gain & Loss Ledger Need to be Configured
        /// </summary>
        FOREXGNL100,
        /// <summary>
        /// Unable to Save 'Forex Gain and Loss' entry under Voucher
        /// </summary>
        FOREXGNL101,
        /// <summary>
        /// POD Received Against this transaction
        /// </summary>
        POD100,
        /// <summary>
        /// Round Off Ledger Need to be Configured
        /// </summary>
        ROUNDOFF100,
        // <summary>
        /// Selected transactions & Group Voucher financial year should be same.
        /// </summary>
        VCH115,
    }

    public class BusinessException : Exception
    {
        //public Exception InnerException { get; set; }
        public BusinessException(ErrorCode errorCode, string extraInfo, Exception exception) : base(exception.Message, exception)
        {
            ErrorCode = errorCode;
            
            if (((BusinessException)exception).ExtraInfo != null && exception.Message == extraInfo) {
                extraInfo = ((BusinessException)exception).ExtraInfo;
            }
            ExtraInfo = extraInfo;

            ODataErrorDetails = new List<ODataErrorDetail>() {new ODataErrorDetail
            {
                ErrorCode = errorCode.ToString(),
                Message = extraInfo,
                Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
            }};
        }

        public BusinessException(string messageid, params object[] parameters)
        {
            parameters = (parameters ?? new[] { "" });
            var message = string.IsNullOrEmpty(messageid) ? null : LiteDBRepository.Instance.GetMessage(messageid);
            if (message == null || (string.IsNullOrWhiteSpace(message?.CustomMessage) && string.IsNullOrWhiteSpace(message?.Message)) || !parameters.Any())
            {
                ErrorCode = ErrorCode.GLB106;
                ExtraInfo = parameters.JoinStrings(",");
            }
            else
            {
                ErrorCode = ErrorCode.GLB106;

                if (string.IsNullOrWhiteSpace(message.CustomMessage))
                {
                    if (message.Message.Contains("{0}"))
                    {
                        message.Message = string.Format(message.Message, parameters[0]);
                    }
                }
                else
                {
                }
            }

            ODataErrorDetails = new List<ODataErrorDetail>() {new ODataErrorDetail
            {
                ErrorCode = ErrorCode.ToString(),
                Message = ExtraInfo,
                Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
            }};

            //var message=LiteDB.
        }

        public BusinessException(ErrorCode errorCode, string extraInfo) : base(extraInfo)
        {
            ErrorCode = errorCode;
            ExtraInfo = extraInfo;
            ODataErrorDetails = new List<ODataErrorDetail>() {new ODataErrorDetail
            {
                ErrorCode = errorCode.ToString(),
                Message = extraInfo,
                Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
            }};
        }

        public BusinessException(ErrorCode errorCode)
        {
            ErrorCode = errorCode;
        }
        public List<ODataErrorDetail> SqlErrors { get; set; } = new List<ODataErrorDetail>();
        public BusinessException(SqlException sqlException) : base(sqlException.Message, sqlException)
        {
            ODataErrorDetails = new List<ODataErrorDetail>();
            if (sqlException.Errors != null)
            {
                foreach(SqlError error in sqlException.Errors)
                {                    
                    SqlErrors.Add(new ODataErrorDetail { 
                    ErrorCode=$"LN{error.LineNumber}, {error.Procedure}",
                    Message=error.Message,
                    Target=error.Source
                    });
                }
            }

            //added by sanjay
            //var msg1 = sqlException.GetBaseException().Message.Split('"');
            if (sqlException.GetBaseException().Message.Contains("CK_TLDateOverlapCheck"))
            {
                ErrorCode = ErrorCode.TAL101;
                ODataErrorDetails.Add(new ODataErrorDetail()
                {
                    ErrorCode = ErrorCode.ToString(),
                    Message = Message,
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                });
            }
            else if (sqlException.GetBaseException().Message.Contains(Resources.NullDateException))
            {
                ErrorCode = ErrorCode.GLB106;
                ODataErrorDetails.Add(new ODataErrorDetail()
                {
                    ErrorCode = ErrorCode.ToString(),
                    Message = "One Date Field has invalide Date",
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                });
            }
            else if (sqlException.GetBaseException().Message.Contains(Resources.DeleteReferencedTransaction))
            {
                ErrorCode = ErrorCode.VCH103;
                ODataErrorDetails.Add(new ODataErrorDetail()
                {
                    ErrorCode = ErrorCode.ToString(),
                    Message = "",
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                });
            }
            else if (sqlException.GetBaseException().GetBaseException().Message.Contains(Resources.UniqIndexFailed))
            {
                ErrorCode = ErrorCode.GLB104;
                var msg = sqlException.GetBaseException().Message;
                //"Cannot insert duplicate key row in object 'dbo.mLedger' with unique index 'IX_Ledger_Alias'.The duplicate key value is (Mani Lal).";
                var part1 = msg.Replace("Cannot insert duplicate key row in object 'dbo.", "").Split('\'');//[0]Table Name And [1]Rest Part
                var tableName = part1[0];
                var indexname = part1[2];
                var value = part1[3].Split('(')[1].Split(')')[0];
                ODataErrorDetails = new List<ODataErrorDetail>
                {
                        new ODataErrorDetail
                        {
                            ErrorCode = ErrorCode.ToString(),
                            Message = $"Cannot insert Duplicate data in {tableName}.{Environment.NewLine}Hint:{indexname}.{Environment.NewLine}Duplicate Value:{value}",
                            Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                        }
                };
            }
            else
            {
                ErrorCode = ErrorCode.GLB106;
                ODataErrorDetails.Add(new ODataErrorDetail()
                {
                    ErrorCode = ErrorCode.ToString(),
                    Message = $"Unable to process request.Please try again.\n Error Details:{sqlException.GetBaseException().Message}",
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode,
                });
            }
        }

        public BusinessException(DbUpdateException sqlException) : base(sqlException.Message, sqlException)
        {
            ODataErrorDetails = new List<ODataErrorDetail>();
            if (sqlException.Entries != null)
            {
                var jsonSetting = new JsonSerializerSettings()
                {
                    DateTimeZoneHandling=DateTimeZoneHandling.Local,
                    MaxDepth=1,
                    ReferenceLoopHandling=ReferenceLoopHandling.Ignore,
                    PreserveReferencesHandling=PreserveReferencesHandling.All
                };
                foreach (var entity in sqlException.Entries)
                {
                    var error = new ODataErrorDetail
                    {
                        ErrorCode = $"State:{entity.State}, {entity.Entity.GetType().Name}",
                        Target = $"Entity:{JsonConvert.SerializeObject(entity.Entity, jsonSetting)}"
                    };
                    try
                    {
                        error.Message = $"OriginalValues:{JsonConvert.SerializeObject(entity.OriginalValues.ToObject(),jsonSetting)}";
                    }
                    catch
                    {
                        //Ignore
                    }
                    try
                    {
                        error.Message += $"CurrentValues:{JsonConvert.SerializeObject(entity.CurrentValues.ToObject(), jsonSetting)}";
                    }
                    catch
                    {
                        //Ignore
                    }
                    SqlErrors.Add(error);
                }
            }
            if (sqlException.GetBaseException().Message.Contains("CK_TLDateOverlapCheck"))
            {
                ErrorCode = ErrorCode.TAL101;
                ODataErrorDetails.Add(new ODataErrorDetail()
                {
                    ErrorCode = ErrorCode.ToString(),
                    Message = Message,
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                });
            }
            else if (sqlException.GetBaseException().Message.Contains(Resources.NullDateException))
            {
                ErrorCode = ErrorCode.GLB106;
                ODataErrorDetails.Add(new ODataErrorDetail()
                {
                    ErrorCode = ErrorCode.ToString(),
                    Message = "One Date Field has invalide Date",
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                });
            }
            else if (sqlException.GetBaseException().Message.Contains(Resources.UniqIndexFailed))
            {
                ErrorCode = ErrorCode.GLB104;
                var msg = sqlException.GetBaseException().Message;
                //"Cannot insert duplicate key row in object 'dbo.mLedger' with unique index 'IX_Ledger_Alias'.The duplicate key value is (Mani Lal).";
                var part1 = msg.Replace("Cannot insert duplicate key row in object '", "").Trim().Split('\'');//[0]Table Name And [1]Rest Part
                var tableName = part1[0];
                var hint = part1[2];
                var value = part1[3].Split('(')[1].Split(')')[0];

                ODataErrorDetails = new List<ODataErrorDetail>
                {
                        new ODataErrorDetail
                        {
                            ErrorCode = ErrorCode.ToString(),
                            Message = $"Cannot insert Duplicate data in {tableName}.{Environment.NewLine}Hint:{hint}.{Environment.NewLine}Duplicate Value:{value}",
                            Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                        }
                };
            }
            else if (sqlException.GetBaseException().Message.Contains(Resources.ForeignKeyFailed))
            {
                //
                ErrorCode = ErrorCode.GLB104;
                var msg = sqlException.GetBaseException().Message.Split('"');
                //"The UPDATE statement conflicted with the FOREIGN KEY constraint "FK_dbo.mVehicleModel_dbo.mGenericMaster_AxleTypeId". The conflict occurred in database "SafeXPlus", table "dbo.mGenericMaster", column 'Id'.";
                var detail = msg[1];
                try
                {
                    detail = detail.Split('_')[3].Replace("Id", "");
                }
                catch (Exception)
                {
                    //Ignore
                }

                ODataErrorDetails = new List<ODataErrorDetail>
                {
                        new ODataErrorDetail
                        {
                            ErrorCode = ErrorCode.ToString(),
                            Message = $"Some Value of Submitted Data was Invalid.{Environment.NewLine}Hint: FOREIGN KEY Voilation {detail}.",
                            Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                        }
                };
            }
            else if (sqlException.GetBaseException().Message.Contains(Resources.RequiredFieldMissing))
            {
                //
                ErrorCode = ErrorCode.GLB104;
                var msg = sqlException.GetBaseException().Message.Split(',');
                //Cannot insert the value NULL into column 'OfficeId', table 'SafeXPlus.dbo.mBook'; column does not allow nulls. INSERT fails.The statement has been terminated.
                var detail = msg[0];

                if (detail.EndsWith("Id'"))
                {
                    detail = detail.Substring(0, detail.Length - 3) + "'";
                }

                ODataErrorDetails = new List<ODataErrorDetail>
                {
                        new ODataErrorDetail
                        {
                            ErrorCode = ErrorCode.ToString(),
                            Message = detail,
                            Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                        }
                };
            }
            else
            {
                //ErrorCode.GLB104, "Update/Insert Failed!!"
                ErrorCode = ErrorCode.GLB104;
                ODataErrorDetails = new List<ODataErrorDetail>() {new ODataErrorDetail()
                {
                    ErrorCode = ErrorCode.ToString(),
                    Message = "Update/Insert failed",
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                },
                new ODataErrorDetail()
                {
                    ErrorCode = ErrorCode.ToString(),
                    Message = sqlException.GetBaseException().Message,
                }};
            }
            
            if (sqlException.Entries != null)
            {
                var builder = new StringBuilder();
                foreach (var result in sqlException.Entries)
                {
                    builder.AppendFormat("Type: {0} was part of the problem. ", result.Entity.GetType().Name);
                }
                var errorString = builder.ToString();
                if (!string.IsNullOrWhiteSpace(errorString))
                {
                    if (ODataErrorDetails != null)
                    {
                        ODataErrorDetails.Add(new ODataErrorDetail()
                        {
                            ErrorCode = ErrorCode.ToString(),
                            Message = errorString
                        });
                    }
                }
            }
        }

        public BusinessException(ErrorCode errorCode, System.ComponentModel.DataAnnotations.ValidationResult validationResult)
        {
            ErrorCode = errorCode;
            if (validationResult != null)
            {
                if (ODataErrorDetails == null) ODataErrorDetails = new List<ODataErrorDetail>();
                ODataErrorDetails.Add(new ODataErrorDetail()
                {
                    ErrorCode = errorCode.ToString(),
                    Message = validationResult.ErrorMessage,
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                });
            }
        }

        public BusinessException(ErrorCode errorCode, DbEntityValidationResult validationResult)
        {
            ErrorCode = errorCode;
            if (validationResult == null || validationResult.IsValid || validationResult.ValidationErrors.Count == 0) return;
            var result = validationResult.ValidationErrors.Select(x => new ODataErrorDetail()
            {
                ErrorCode = errorCode.ToString(),
                Message = $"{x.PropertyName}:{x.ErrorMessage}",
                Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
            }).ToList();
            if (ODataErrorDetails == null) { ODataErrorDetails = new List<ODataErrorDetail>(); }
            ODataErrorDetails.AddRange(result);
        }

        public BusinessException(ErrorCode errorCode, IEnumerable<DbEntityValidationResult> validationResult)
        {
            ErrorCode = errorCode;
            var dbEntityValidationResults = validationResult as IList<DbEntityValidationResult> ?? validationResult?.ToList();
            if (validationResult == null || !dbEntityValidationResults.Any()) return;
            ODataErrorDetails =
                dbEntityValidationResults.SelectMany(validationError => validationError.ValidationErrors)
                    .Select(x => new ODataErrorDetail()
                    {
                        ErrorCode = errorCode.ToString(),
                        Message = $"Property:{x.PropertyName}:Error:{x.ErrorMessage}",
                        Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                    }).ToList();
        }

        public BusinessException(ErrorCode errorCode, List<ValidationResult> validationResult)
        {
            ErrorCode = errorCode;
            if (!validationResult.Any()) return;
            ODataErrorDetails = new List<ODataErrorDetail>();
            foreach (var result in validationResult)
            {
                ODataErrorDetails.Add(new ODataErrorDetail()
                {
                    ErrorCode = errorCode.ToString(),
                    Message = $"{result.ErrorMessage}=>[{result.MemberNames.JoinStrings(",")}]",
                    Target = "https://africa.indiaweblab.com/fwlink?errorcode=" + ErrorCode
                });
            }
        }

        public ErrorCode ErrorCode { get; private set; }
        public string ExtraInfo { get; set; }
        public HttpStatusCode HttpStatusCode { get; set; }
        /// <exception cref="ArgumentOutOfRangeException" accessor="get">Enum Not Implemented.</exception>
        public override string Message
        {
            get
            {
                string message;
                HttpStatusCode = HttpStatusCode.BadRequest;
                switch (ErrorCode)
                {
                    case ErrorCode.GLB112:
                        message = "Transaction has been approved.Hint FormId:1753";
                        break;
                    case ErrorCode.ROUNDOFF100:
                        message = "Default 'Round Off' ledger need to be configured.";
                        break;
                    case ErrorCode.FOREXGNL100:
                        message = "Default 'Forex Gain and Loss' ledger need to be configured.";
                        break;
                    case ErrorCode.FOREXGNL101:
                        message = "Unable to Save 'Forex Gain and Loss' entry under Voucher";
                        break;
                    case ErrorCode.VCH100:
                        message = "The Document date does not exists between any financial year.";
                        break;

                    case ErrorCode.VCH101:
                        message = "Cannot Modify Approved Transaction";
                        break;

                    case ErrorCode.VCH102:
                        message = "The Audited Voucher Transaction cannot be deleted.";
                        break;

                    case ErrorCode.VCH103:
                        message = "Used Reference cannot be deleted";
                        break;

                    case ErrorCode.TADV100:
                        message = "Validation failed for Amount.";
                        break;

                    case ErrorCode.TADV101:
                        message = "At least two VD are required in Advance Transaction Voucher";
                        break;

                    case ErrorCode.TADV102:
                        message = "At lead one VDR is Required in Advance Transaction";
                        break;

                    case ErrorCode.TADV103:
                        message = "VD and VDR Amount Doesn't Tally";
                        break;

                    case ErrorCode.TADV104:
                        message = "Divide By Zero Error";
                        break;

                    case ErrorCode.GLB100:
                        message = "Session Expired";
                        HttpStatusCode = HttpStatusCode.Unauthorized;
                        break;

                    case ErrorCode.VCH104:
                        message = "Credit and Debit Amount mismatch for Voucher";
                        break;

                    case ErrorCode.VCH105:
                        message = "At least two Voucher Details are required in Voucher";
                        break;

                    case ErrorCode.VCH106:
                        message = "VoucherDetail and VoucherDetailReference Amount Doesn't Tally";
                        break;

                    case ErrorCode.TADV105:
                        message = "Cannot Alter Settled Trip Advance";
                        break;

                    case ErrorCode.TAL100:
                        message = "Cannot to Delete Settled TripLog";
                        break;

                    case ErrorCode.GLB102:
                        message = "Authentication Failed";
                        HttpStatusCode = HttpStatusCode.Unauthorized;
                        break;

                    case ErrorCode.TS100:
                        message = "Bad Data in Attached Advances";
                        break;

                    case ErrorCode.TS101:
                        message = "Bad Data in Attached Trip Expanses";
                        break;

                    case ErrorCode.TS102:
                        message = "Bad Data in Attached Vehicle Movement logs(TripLogs)";
                        break;

                    case ErrorCode.TS103:
                        message = "Bad Data in Attached Fuel Expanses";
                        break;

                    case ErrorCode.VCH107:
                        message = "Invalid Data in Transaction[VDR]";
                        break;

                    case ErrorCode.TEXP100:
                        message = "Invalid Operation of Trip Expanse Entity";
                        break;

                    case ErrorCode.VCH108:
                        message = "Invalid VoucherId";
                        break;

                    case ErrorCode.DUET100:
                        message = "Unable to generate prepaid tax entry";
                        break;

                    case ErrorCode.VCH109:
                        message = "Insufficient amount available for against reference entry";
                        break;

                    case ErrorCode.DUET101:
                        message = "Expiry date should be greater than or equal to Start Date";
                        break;

                    case ErrorCode.DUET102:
                        message = "Start Date should be greater than or equal to Paid Date and Start Date and Paid Date should be in same Financial Year.";
                        break;

                    case ErrorCode.DUET103:
                        message = "PrePaid Tax Transaction entry already exists";
                        break;

                    case ErrorCode.GLB103:
                        message = "Configuration not found for specified key";
                        break;

                    case ErrorCode.GLB104:
                        message = "Data Conflict Occurred";
                        break;

                    case ErrorCode.GLB105:
                        message = "Cannot modify referenced Transaction";
                        break;

                    case ErrorCode.VCH110:
                        message = "Invalid Ledger/Account";
                        break;

                    case ErrorCode.VCH111:
                        message = "At least One VoucherDetailReference is required.";
                        break;

                    case ErrorCode.SPB100:
                        message = "Bad Data in Spare Details of Transaction";
                        break;

                    case ErrorCode.SPB101:
                        message = "Bad Data in Labour Details of Transaction";
                        break;

                    case ErrorCode.GLB106:
                        message = "Validation Failed";
                        break;

                    case ErrorCode.SPB102:
                        message = "Insufficient Balance Qty";
                        break;

                    case ErrorCode.SPB103:
                        message = "Issued/Transfered Item cannot be deleted.";
                        break;

                    case ErrorCode.GLB107:
                        HttpStatusCode = HttpStatusCode.Forbidden;
                        message = "Request Rejected";
                        break;

                    case ErrorCode.GLB108:
                        message = "Used Transaction cannot be deleted";
                        break;

                    case ErrorCode.TYR100:
                        message = "Invalid TyreId";
                        break;

                    case ErrorCode.TYR101:
                        message = "Invalid Parent Indentifier";
                        break;

                    case ErrorCode.TYR102:
                        message = "Invalid Tyre Number";
                        break;

                    case ErrorCode.TYR103:
                        message = "Tyre Transaction Date is Invalid";
                        break;

                    case ErrorCode.VEH100:
                        message = "Cannot Save Chassis Tyres where Vehicle Owner is not defined";
                        break;

                    case ErrorCode.GLB109:
                        message = "Not Found";
                        break;

                    case ErrorCode.VCH112:
                        message = "Financial year is Locked";
                        break;

                    case ErrorCode.GLB110:
                        message = "Error in executing Report at Server";
                        break;

                    case ErrorCode.GLB111:
                        message = "Internal Configuration can't be updated";
                        break;

                    case ErrorCode.TADV106:
                        message = "Trip Advance has already been settled in another Settlement";
                        break;

                    case ErrorCode.TADV107:
                        message = "The Vehicle in Trip Advance and Vehicle in mapped Trip should be same.";
                        break;

                    case ErrorCode.JobFailed:
                        message = "Job Failed";
                        break;

                    case ErrorCode.EventFailed:
                        message = "Event Faild";
                        break;

                    case ErrorCode.TAL101:
                        message = "Cannot update/Insert this transaction as given dates overlap with other transaction";
                        break;

                    case ErrorCode.VCH113:
                        message = "Zero amount Transaction are not allowed in accounting";
                        break;

                    case ErrorCode.VCH114:
                        message = "Account has been partially locked";
                        break;
                    case ErrorCode.TADV108:
                        message = "The Trip Advance has been Disbursed and cannot be updated or deleted";
                        break;
                    case ErrorCode.CUR100:
                        message = "Currency rate is required";
                        break;
                    case ErrorCode.POD100:
                        message = "POD has been received against this transaction";
                        break;
                    case ErrorCode.VCH115:
                        message = "Selected transactions &Group Voucher financial year should be same.";
                        break;
                        
                    default:
                        throw new ArgumentOutOfRangeException();
                }
                return message;
            }
        }

        public List<ODataErrorDetail> ODataErrorDetails { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}