using System.Collections.Generic;

namespace TrackoAPI.ViewModels.Global
{
    public class vwConstantType : BaseEntity<int>
    {
        public string ConstantTypeAbbr { get; set; }
        public string ConstantTypeName { get; set; }
        public string ConstantTypeDesc { get; set; }
        public virtual ICollection<vwConstantValue> ConstantValues { get; set; }
        public bool IsDepricated { get; set; }
    }
    public class vwConstantValue : BaseEntity<int>
    {
        public vwConstantValue()
        {
        }
        public string ConstantAbbr { get; set; }
        public string ConstantName { get; set; }
        public int ConstantTypeId { get; set; }
        public virtual vwConstantType ContanType { get; set; }
        public string ConstantRemarks { get; set; }
        public int Visiblity { get; set; }
        public bool IsDepricated { get; set; }
    }
}
