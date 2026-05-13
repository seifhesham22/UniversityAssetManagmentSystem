using FluentValidation;

namespace UAMS.Campus.Features.AdminFeatures.CreateAssetManagerProfile
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
