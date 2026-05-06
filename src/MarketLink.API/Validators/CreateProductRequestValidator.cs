using FluentValidation;
using MarketLink.Application.Models.Product;

namespace MarketLink.API.Validators
{
    public class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
    {
        public CreateProductRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Mahsulot nomi bo'sh bo'lishi mumkin emas")
                .MaximumLength(200).WithMessage("Mahsulot nomi 200 belgidan oshmasligi kerak");

            RuleFor(x => x.Description)
                .MaximumLength(2000).WithMessage("Tavsif 2000 belgidan oshmasligi kerak")
                .When(x => x.Description != null);

            RuleFor(x => x.Price)
                .GreaterThan(0).WithMessage("Narx 0 dan katta bo'lishi kerak");

            RuleFor(x => x.PackageSize)
                .GreaterThan(0).WithMessage("O'ram soni 0 dan katta bo'lishi kerak");

            RuleFor(x => x.StockQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("Qoldiq miqdori manfiy bo'lishi mumkin emas");

            RuleFor(x => x.Image)
                .Must(f => f == null || f.Length <= 10 * 1024 * 1024)
                .WithMessage("Rasm hajmi 10 MB dan oshmasligi kerak")
                .Must(f => f == null || IsValidImageType(f.ContentType))
                .WithMessage("Faqat JPG, PNG, WEBP yoki GIF formatdagi rasmlar qabul qilinadi");
        }

        private static bool IsValidImageType(string contentType) =>
            contentType is "image/jpeg" or "image/png" or "image/webp" or "image/gif";
    }
}
