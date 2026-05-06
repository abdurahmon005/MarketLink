using FluentValidation;
using MarketLink.Application.Models.Rating;

namespace MarketLink.API.Validators
{
    public class RateProductDtoValidator : AbstractValidator<RateProductDto>
    {
        public RateProductDtoValidator()
        {
            RuleFor(x => x.OrderId)
                .GreaterThan(0).WithMessage("OrderId noto'g'ri");

            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId noto'g'ri");

            RuleFor(x => x.Score)
                .InclusiveBetween(1, 5).WithMessage("Baho 1 dan 5 gacha bo'lishi kerak");

            RuleFor(x => x.Comment)
                .MaximumLength(1000).WithMessage("Izoh 1000 belgidan oshmasligi kerak")
                .When(x => x.Comment != null);
        }
    }
}
