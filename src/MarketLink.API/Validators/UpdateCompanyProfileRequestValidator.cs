using FluentValidation;
using MarketLink.Application.Models.Company;

namespace MarketLink.API.Validators
{
    public class UpdateCompanyProfileRequestValidator : AbstractValidator<UpdateCompanyProfileRequest>
    {
        public UpdateCompanyProfileRequestValidator()
        {
            RuleFor(x => x.FounderName)
                .MaximumLength(100).WithMessage("Asoschining ismi 100 belgidan oshmasligi kerak")
                .When(x => x.FounderName != null);

            RuleFor(x => x.CompanyName)
                .MaximumLength(150).WithMessage("Kompaniya nomi 150 belgidan oshmasligi kerak")
                .When(x => x.CompanyName != null);

            RuleFor(x => x.Address)
                .MaximumLength(300).WithMessage("Manzil 300 belgidan oshmasligi kerak")
                .When(x => x.Address != null);

            RuleFor(x => x.Description)
                .MaximumLength(1000).WithMessage("Tavsif 1000 belgidan oshmasligi kerak")
                .When(x => x.Description != null);

            RuleFor(x => x.ProductionType)
                .IsInEnum().WithMessage("Noto'g'ri ishlab chiqarish turi")
                .When(x => x.ProductionType != null);
        }
    }
}
