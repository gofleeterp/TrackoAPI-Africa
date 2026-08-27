using System;
using System.Linq;
using TrackoApi.Models.AMS;
using TrackoApi.Models.Base;

namespace TrackoApi.Models.Validations.AMS
{
    public class AccountValidations
    {
        public static Predicate<VoucherDetail> VoucherDetailAmountIsValid = vd =>
        {

            if (vd.VoucherDetailReferences.Count(x => x.ObjectState != ObjectState.Deleted) > 0)
            {
                return vd.Amount ==
                vd.VoucherDetailReferences.Where(x => x.ObjectState != ObjectState.Deleted)
                    .Sum(x => x.Amount);
            }
            return true;
        };
        public static Predicate<Voucher> VoucherAmountIsValid = voucher =>
        {
            return ((voucher.Amount1 + voucher.Amount2 + voucher.Amount3 + voucher.Amount4 +
                   voucher.Amount5 + voucher.Amount6) == 0) && (voucher.VoucherDetails.Sum(x => x.Amount) == 0);
        };
    }
}
