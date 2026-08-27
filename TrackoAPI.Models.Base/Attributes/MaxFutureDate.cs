using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackoApi.Models.Base.Attributes
{
    public class MaxFutureDate: ValidationAttribute
    {
        private DateTime MaxAllowedDate = DateTime.Now;
        public MaxFutureDate(string ErrorMessage= "Date for Field {0} cannot be greater than {1:L}") :base(ErrorMessage)
        {

        }
        public MaxFutureDate(int MaxFutureDaysAllowed=0, string ErrorMessage = "Date for Field {0} cannot be greater than {1:L}") : base(ErrorMessage)
        {
            MaxAllowedDate = DateTime.Now.AddDays(MaxFutureDaysAllowed).AddMinutes(30);
        }
        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }
            DateTime? date = value as DateTime?;
            if (date == null) return true;
            if (date.Value > MaxAllowedDate) return false;
            return true;
        }
        public override string FormatErrorMessage(string name)
        {
            return string.Format(CultureInfo.CurrentCulture, base.ErrorMessageString, new object[2]
            {
                name,
                MaxAllowedDate
            });
        }
    }
}
