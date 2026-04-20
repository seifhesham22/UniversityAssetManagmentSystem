using FluentValidation;

namespace UAMS.Campus.Features.CreateBuilding
{
    public sealed class CreateBuildingValidator : AbstractValidator<CreateBuildingCommand>
    {
        public CreateBuildingValidator()
        {
            RuleFor(x => x.address).NotEmpty().NotNull();
            RuleFor(x => x.name).NotEmpty().NotNull();
        }
    }
}