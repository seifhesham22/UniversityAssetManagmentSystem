using FluentValidation;
using Shared.Enums;

namespace UAMS.Room.Features.CreateAssetDefinition
{
    public sealed class CreateAssetDefinitionValidator : AbstractValidator<CreateAssetDefinitionCommand>
    {
        public CreateAssetDefinitionValidator()
        {
            RuleFor(x => x.name)
                .NotNull()
                .MaximumLength(60);

            RuleFor(x => x.Category)
                .Must(category => Enum.IsDefined(typeof(AssetCategory), category))
                .WithMessage(
                $"category must be in {Enum.GetName<AssetCategory>}");

            RuleFor(x => x.Locations).NotNull();
        }
    }
}