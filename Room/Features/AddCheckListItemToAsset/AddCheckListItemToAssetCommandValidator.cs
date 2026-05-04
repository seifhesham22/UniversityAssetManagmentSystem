using FluentValidation;

namespace UAMS.Room.Features.AddCheckListItemToAsset
{
    public sealed class AddCheckListItemToAssetCommandValidator
        : AbstractValidator<AddCheckListItemToAssetCommand>
    {
        public AddCheckListItemToAssetCommandValidator()
        {
            RuleFor(x => x.Description)
                .NotEmpty()
                .NotNull()
                .MaximumLength(60);
        }
    }
}