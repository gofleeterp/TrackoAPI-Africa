using System;
using System.ComponentModel.DataAnnotations;

namespace ModelValidations.Attributes
{
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public class MinimumAttribute: ValidationAttribute
    {
        /// <summary>
        /// Gets the minimum allowable value of the data.
        /// </summary>
        public double Minimum { get; private set; }

        public MinimumAttribute(double min,string errorMessage=null) : base(string.IsNullOrWhiteSpace(errorMessage)?$"Value should be greater than or equal to {min}.":errorMessage)
        {
            Minimum = min;
        }

        public override bool IsValid(object value)
        {
            if (value == null)
            {
                return true;
            }
            double min = 0;
            if (!double.TryParse(value.ToString(), out min))
            {
                return false;
            }
            return !(min<Minimum);
        }
    }
}
