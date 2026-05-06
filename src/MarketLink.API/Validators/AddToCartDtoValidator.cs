using FluentValidation;
using MarketLink.Application.Models.Cart;

namespace MarketLink.API.Validators
{
    public class AddToCartDtoValidator : AbstractValidator<AddToCartDto>
    {
        public AddToCartDtoValidator()
        {
            RuleFor(x => x.ProductId)
                .GreaterThan(0).WithMessage("ProductId noto'g'ri");

            RuleFor(x => x.Quantity)
                .GreaterThan(0).WithMessage("Miqdor 0 dan katta bo'lishi kerak")
                .LessThanOrEqualTo(1000).WithMessage("Miqdor 1000 dan oshmasligi kerak");
        }
    }
}
