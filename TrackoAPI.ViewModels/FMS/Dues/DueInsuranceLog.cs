using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Core.Objects.DataClasses;

namespace TrackoAPI.ViewModels.FMS.Dues
{
    [ComplexType, EdmComplexType]
    public class vwDueInsuranceLog
    {
        public long Id { get; set; }
        public long? InsCompanyId { get; set; }
        [MaxLength(100)]
        public string AgentName { get; set; }
        [MaxLength(20)]
        public string InsOfficerName { get; set; }
        public decimal InsuredValue { get; set; } = 0;
        public decimal Compulsory { get; set; } = 0;
        public decimal TPPremium { get; set; } = 0;
        public long PACCount { get; set; } = 0;
        public decimal PACValue { get; set; } = 0;      

        public decimal Premium { get; set; } = 0;
        public decimal ImposedValue { get; set; } = 0;
        public long GVWOD { get; set; } = 0;
        public decimal Discount { get; set; } = 0;
        public decimal NCBPercent { get; set; } = 0;
        public decimal NCBAmount { get; set; } = 0;
        
        public bool IsComprehensive { get; set; }
        public string InsCompanyName { get; set; }
    }
}
