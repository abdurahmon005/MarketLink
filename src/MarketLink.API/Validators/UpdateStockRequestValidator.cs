using FluentValidation;
using MarketLink.Application.Models.Supplier;

namespace MarketLink.API.Validators
{
    public class UpdateStockRequestValidator : AbstractValidator<UpdateStockRequest>
    {
        public UpdateStockRequestValidator()
        {
            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Miqdor 0 dan katta bo'lishi kerak");

            RuleFor(x => x.Reason)
                .NotEmpty().WithMessage("Sabab ko'rsatilishi shart");

            RuleFor(x => x.ChangeType)
                .Must(t => t == "add" || t == "set")
                .WithMessage("ChangeType 'add' yoki 'set' bo'lishi kerak");
        }
    }
}
