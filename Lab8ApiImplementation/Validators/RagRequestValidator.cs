using FluentValidation;
using Lab8ApiImplementation.Dtos;

namespace Lab8ApiImplementation.Validators
{
    public class RagRequestValidator : AbstractValidator<RagRequest>
    {
        public RagRequestValidator()
        {
            RuleFor(x => x.Question)
                .NotEmpty().WithMessage("A question is required.")
                .MaximumLength(1000).WithMessage("A question can be at most 1000 characters.");
        }
    }
}
