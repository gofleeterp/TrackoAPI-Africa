using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.OData.Builder;
using Microsoft.OData.Edm;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;
using TrackoApi.Models.Global;
using TrackoApi.Models.Validations;

namespace TrackoApi.Models.BMS
{
    [Table("tCNStatusLog")]
    public class CnStatusLog:AuditableEntity
    {
        public long CNId { get; set; }
        [ForeignKey("CNId"),ActionOnDelete(EdmOnDeleteAction.Cascade)]
        public CNMaster fk_CNMaster { get; set; }
        [DataType(DataType.Date)]
        public DateTime DocDate { get; set; }
        public long? DocId { get; set; }
        [MaxLength(300), StationaryCheck]
        public string DocNumber { get; set; }
        public long PartyId { get; set; }
        [ForeignKey("PartyId")]
        public Ledger fk_Party { get; set; }
        public long OfficeId { get; set; }
        [ForeignKey("OfficeId")]
        public OfficeMaster fk_Office { get; set; }
        /// <summary>
        /// Gets or sets the document type identifier.
        /// <remarks>Constant Type Id 109</remarks>
        /// </summary>
        /// <value>The document type identifier.</value>
        public long DocTypeId { get; set; }
        [ForeignKey("DocTypeId")]
        public ConstantValue fk_DocType { get; set; }
        [MaxLength(300)]
        public string TransactionRemark { get; set; }


    }
}
