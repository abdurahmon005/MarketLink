using FluentValidation;
using MarketLink.Application.Models.Product;

namespace MarketLink.API.Validators
{
    public class UpdateProductRequestValidator : AbstractValidator<UpdateProductRequest>
    {
        public UpdateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Mahsulot nomi bo'sh bo'lishi mumkin emas")
                .MaximumLength(200).WithMessage("Mahsulot nomi 200 belgidan oshmasligi kerak")
                .When(x => x.Name != null);

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Tavsif 2000 belgidan oshmasligi kerak")
                .When(x => x.Description != null);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Narx 0 dan katta bo'lishi kerak")
                .When(x => x.Price != null);

            RuleFor(x => x.PackageSize)
                .GreaterThan(0).WithMessage("O'ram soni 0 dan katta bo'lishi kerak")
                .When(x => x.PackageSize != null);

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Qoldiq miqdori manfiy bo'lishi mumkin emas")
                .When(x => x.StockQuantity != null);
        }
    }
}
