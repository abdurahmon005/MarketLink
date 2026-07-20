using FluentValidation;
using MarketLink.Application.Models.Supplier;

namespace MarketLink.API.Validators
{
    public class AssignDriverRequestValidator : AbstractValidator<AssignDriverRequest>
    {
        public AssignDriverRequestValidator()
        {
            RuleFor(x => x.DriverId)
                .NotEmpty().WithMessage("Haydovchi ID si kiritilishi shart");

            RuleFor(x => x.DriverName)
                .NotEmpty().WithMessage("Haydovchi ismi kiritilishi shart")
                .MaximumLength(100).WithMessage("Haydovchi ismi 100 belgidan oshmasligi kerak");

            RuleFor(x => x.DriverPhone)
                .NotEmpty().WithMessage("Haydovchi telefon raqami kiritilishi shart");

            RuleFor(x => x.EstimatedMinutes)
                .GreaterThan(0).WithMessage("Taxminiy vaqt 0 dan katta bo'lishi kerak");
        }
    }
}
