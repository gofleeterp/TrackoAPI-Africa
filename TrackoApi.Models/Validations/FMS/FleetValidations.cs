using FluentValidation;
using TrackoApi.Models.FMS;

namespace TrackoApi.Models.Validations.FMS
{
    public class SpareLogValidator:AbstractValidator<SpareLog>
    {
        public SpareLogValidator()
        {
            RuleFor(x => x.Amount).Equal(x => x.Rate * x.Qty).WithMessage("Amount: Should be eq to [Rate*Qty]");
            RuleFor(x => x.DiscountAmount)
                .Equal(x => x.Amount*x.DiscountPercent/100)
                .WithMessage("Discount Amount is invalid");
            RuleFor(x => x.SparePartId).NotNull().NotEqual(0).WithMessage("Spare Item Value is Missing");
        }
//        var validator = new SpareLogValidator();
//        var result = validator.Validate(a);
//                if (!result.IsValid)
//                {
//                    var validations = new List<ValidationResult>();
//                    foreach (var error in result.Errors)
//                    {
//                        var val = new ValidationResult(error.ErrorMessage, new[] { error.PropertyName });
//        validations.Add(val);
//                    }
//                    throw new BusinessException(ErrorCode.SPB100, validations);
//}
    }
}
