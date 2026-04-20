using FluentValidation;

namespace UAMS.Campus.Features.CreateDeapartment
{
    public sealed class CreateDepartmentValidator : AbstractValidator<CreateDepartmentCommand>
    {
        public CreateDepartmentValidator()
        {
            RuleFor(x => x.handles).IsInEnum();
            RuleFor(x => x.name).NotNull().NotEmpty();
        }
    }
}