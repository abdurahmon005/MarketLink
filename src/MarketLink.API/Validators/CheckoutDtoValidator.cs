using FluentValidation;
using MarketLink.Application.Models.Order;

namespace MarketLink.API.Validators
{
    public class CheckoutDtoValidator : AbstractValidator<CheckoutDto>
    {
        public CheckoutDtoValidator()
        {
            RuleFor(x => x.DeliveryDate)
                .GreaterThan(DateTime.UtcNow.Date)
                .WithMessage("Yetkazish sanasi bugundan keyin bo'lishi kerak");

            RuleFor(x => x.DeliveryAddress)
                .NotEmpty().WithMessage("Yetkazish manzili bo'sh bo'lishi mumkin emas")
                .MaximumLength(500).WithMessage("Manzil 500 belgidan oshmasligi kerak");
        }
    }
}
