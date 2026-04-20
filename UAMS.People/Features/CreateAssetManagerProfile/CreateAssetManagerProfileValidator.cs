using FluentValidation;

namespace UAMS.Campus.Features.CreateAssetManagerProfile
{
    public sealed class CreateAssetManagerProfileValidator
        : AbstractValidator<CreateAssetManagerProfileCommand>
    {
        public CreateAssetManagerProfileValidator()
        {
            RuleFor(x => x.fullName)
                .NotEmpty()
                .MaximumLength(60);
        }
    }
}
