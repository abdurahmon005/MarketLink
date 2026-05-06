using FluentValidation;
using MarketLink.Application.Models.Shop;

namespace MarketLink.API.Validators
{
    public class UpdateShopProfileRequestValidator : AbstractValidator<UpdateShopProfileRequest>
    {
        public UpdateShopProfileRequestValidator()
        {
            RuleFor(x => x.FounderName)
                .MaximumLength(100).WithMessage("Asoschining ismi 100 belgidan oshmasligi kerak")
                .When(x => x.FounderName != null);

            RuleFor(x => x.ShopName)
                .MaximumLength(150).WithMessage("Do'kon nomi 150 belgidan oshmasligi kerak")
                .When(x => x.ShopName != null);

            RuleFor(x => x.Address)
                .MaximumLength(300).WithMessage("Manzil 300 belgidan oshmasligi kerak")
                .When(x => x.Address != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Tavsif 1000 belgidan oshmasligi kerak")
                .When(x => x.Description != null);

            RuleFor(x => x.ShopType)
                .IsInEnum().WithMessage("Noto'g'ri do'kon turi")
                .When(x => x.ShopType != null);
        }
    }
}
