using FluentValidation;

namespace UAMS.Campus.Features.AdminFeatures.CreateFaculty
{
    public sealed class CreateFacultyValidator : AbstractValidator<CreateFacultyCommand>
    {
        public CreateFacultyValidator()
        {
            RuleFor(x => x.name).NotEmpty();
        }
    }
}