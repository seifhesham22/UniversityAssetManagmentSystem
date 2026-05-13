using FluentValidation;

namespace UAMS.Campus.Features.AdminFeatures.CreateDepartmentManagerProfile
{
    public sealed class CreateDepartmentManagerProfileValidator 
        : AbstractValidator<CreateDepartmentManagerCommand>
    {
        public CreateDepartmentManagerProfileValidator()
        {
            RuleFor(x => x.fullName).NotEmpty().MaximumLength(60);
        }
    }
}
