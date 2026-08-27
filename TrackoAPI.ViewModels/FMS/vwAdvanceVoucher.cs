using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using TrackoAPI.ViewModels.Global;

namespace TrackoAPI.ViewModels.FMS
{
    public class vwAdvanceVoucher
    {
        [Key]
        public long Id { get; set; }
        public long? PageId { get; set; }
        public string DocumentNo { get; set; }
        public long CrAccountId { get; set; }
        public string CrAccountName { get; set; }
        public long DrAccountId { get; set; }
        public string DrAccountName { get; set; }
        public DateTime DocumentDate{ get; set; }
        public long? HSNCodeId { get; set; }
        public string HSNCode { get; set; }

        public long? IGSTAccountId { get; set; }
        public string IGSTAccountName { get; set; }
        public long? CGSTAccountId { get; set; }
        public string CGSTAccountName { get; set; }
        public long? SGSTAccountId { get; set; }
        public string SGSTAccountName { get; set; }
        public long OfficeId { get; set; }
        public string OfficeName { get; set; }
        public string Remark { get; set; }
        public long AdvanceTypeId { get; set; }
        public string AdvanceType { get; set; }
        public decimal NetAmount { get; set; }
        public List<vwTripAdvanceLog> TripAdvanceLogs { get; set; }
        public bool IsLocked { get; set; }
        public long? ViewId { get; set; }

        public string BatchId { get; set; }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
    }
    public class vwTripAdvanceLog
    {
        public long AdvanceId { get; set; }
        public string VoucherNo { get; set; }
        public string PaidIn { get; set; }
        public long? VehicleId { get; set; }
        public string VehicleNo { get; set; }
        public long? HireVehicleId { get; set; }
        public string HireVehicleNo { get; set; }
        public DateTime AdvanceDate { get; set; }
        public long OfficeId { get; set; }
        public string OfficeName { get; set; }
        public long? CreditAccountId { get; set; }
        public string CreditAccount { get; set; }
        public long DebitAccountId { get; set; }
        public string DebitAccountName { get; set; }
        public decimal CashAmount { get; set; } = 0;
        public long? ExpenseId { get; set; }
        public string Expense { get; set; }
        public long? FuelId { get; set; }
        public string FuelTypeName { get; set; }
        public decimal FuelQty { get; set; } = 0;
        public decimal FuelRate { get; set; } = 0;
        public decimal FuelAmount { get; set; } = 0;
        public long? SettlementId { get; set; }
        public string SettlementNo { get; set; }
        public long? SettledRefId { get; set; }
        public long? DriverId { get; set; } = 0;
        public string DriverName { get; set; }
        public string Remark { get; set; }
        public long? TripLogId { get; set; }
        public string TripLogNo { get; set; }
        public long? AdvanceTypeId { get; set; }
        public string ReferenceNo { get; set; }
        public long? VoucherId { get; set; }
        public decimal Amount { get { return CashAmount + FuelAmount; } set {} }
        public long? ViewId { get; set; }
        public long? PaidInId{ get; set; }
        public string Ref1 { get; set; }
        public string ThirdPartyRefNo { get; set; }
        public long? HSNCodeId { get; set; }
        public string HSNCode { get; set; }

        public long? IGSTAccountId { get; set; }
        public string IGSTAccountName { get; set; }
        public decimal IGSTRate { get; set; }
        public decimal IGSTAmount { get; set; }

        public long? CGSTAccountId { get; set; }
        public string CGSTAccountName { get; set; }
        public decimal CGSTRate { get; set; }
        public decimal CGSTAmount { get; set; }

        public long? SGSTAccountId { get; set; }
        public string SGSTAccountName { get; set; }
        public decimal SGSTRate { get; set; }
        public decimal SGSTAmount { get; set; }
        public decimal NetAmount { get; set; }
        public long? CurTypeId { get; set; }
        public long? ConstCurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
        public string Data { get; set; }
        private List<JsonDataEntity> _dt;
        public List<JsonDataEntity> DataView
        {
            get => _dt == null||!_dt.Any() ? (string.IsNullOrWhiteSpace(Data) ? new List<JsonDataEntity>() : JsonConvert.DeserializeObject<List<JsonDataEntity>>(Data)) : _dt;
            set
            {
                _dt = value;
                Data = value == null || value.Count == 0 ? "[]" : JsonConvert.SerializeObject(value);
            }
        }
    }
    
}
