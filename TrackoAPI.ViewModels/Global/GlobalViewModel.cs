using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoAPI.ViewModels.Global
{
    public class vwTSL
    {
        public long Id { get; set; } = 0;
        public long ViewId { get; set; } = 0;
        public long RecordId { get; set; } = 0;
        public string KeyValue { get; set; }
        public string TextValue1 { get; set; }
        public string TextValue2 { get; set; }
        public DateTime? DocDate { get; set; }
        public long? Ref1Id { get; set; }
        public string Ref1Name { get; set; }

        public long? Ref2Id { get; set; }
        public string Ref2Name { get; set; }

        public long? Const1Id { get; set; }
        public string Const1Value { get; set; }
        public long? Const2Id { get; set; }
        public string Const2Value { get; set; }
        public long? Generic1Id { get; set; }
        public string Generic1Value { get; set; }
        public long? Generic2Id { get; set; }
        public string Generic2Value { get; set; }
        public string RefI { get; set; }
        public string RefII { get; set; }
        public decimal Value1 { get; set; } = 0;
        public decimal Value2 { get; set; } = 0;
        public decimal Value3 { get; set; } = 0;
        public string JsonData { get; set; }
        public long? CurTypeId { get; set; }
        public decimal CurRate { get; set; } = 0;
        public string Remarks { get; set; }
        public bool IsDeletedId { get; set; } = false;
    }
}
