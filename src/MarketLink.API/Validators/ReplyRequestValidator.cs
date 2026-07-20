using FluentValidation;
using MarketLink.Application.Models.Supplier;

namespace MarketLink.API.Validators
{
    public class ReplyRequestValidator : AbstractValidator<ReplyRequest>
    {
        public ReplyRequestValidator()
        {
            RuleFor(x => x.Reply)
                .NotEmpty().WithMessage("Javob bo'sh bo'lmasligi kerak")
                .MaximumLength(500).WithMessage("Javob 500 belgidan oshmasligi kerak");
        }
    }
}
