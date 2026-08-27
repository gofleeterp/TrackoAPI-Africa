using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.OData.Edm.Library;
using System.Data.Entity.Core.Objects.DataClasses;
using System.Dynamic;
using System.ComponentModel.DataAnnotations.Schema;

namespace TrackoAPI.ViewModels.FMS.Driver
{
    [EdmComplexType]
    public class vwFleetAccount
    {
        public string AccountName { get; set; }
        public string BookingAcName { get; set; }
        public long? GroupId { get; set; }
        public bool IsDefaulter { get; set; }
        [MaxLength(10)]
        public string PanNo { get; set; }
        public long? RoleId { get; set; }
        #region bank 1
        public long? Bank1Id { get; set; }

        [MaxLength(150)]
        public string BankAcccoutName1 { get; set; }

        [MaxLength(100)]
        public string BankAcccoutNo1 { get; set; }

        [ MaxLength(80)]
        public string BankCode1 { get; set; }
        [MaxLength(80)]
        public string BankSwiftCode1 { get; set; }

        [MaxLength(15)]
        public string BankUPI1 { get; set; }

        [MaxLength(1500)]
        public string BankAdd1 { get; set; }
        #endregion
        #region Account2 Outside currency bank
        public long? Bank2Id { get; set; }

        [MaxLength(150)]
        public string BankAcccoutName2 { get; set; }

        [MaxLength(100)]
        public string BankAcccoutNo2 { get; set; }

        [MaxLength(80)]
        public string BankCode2 { get; set; }
        [MaxLength(80)]
        public string BankSwiftCode2 { get; set; }

        [ MaxLength(15)]
        public string BankUPI2 { get; set; }

        [MaxLength(1500)]
        public string BankAdd2 { get; set; }

        public long? CurTypeId { get; set; }
        #endregion

    }
}
