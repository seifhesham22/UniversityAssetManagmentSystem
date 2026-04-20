using FluentValidation;

namespace UAMS.Campus.Features.CreateFaculty
{
    public sealed class CreateFacultyValidator : AbstractValidator<CreateFacultyCommand>
    {
        public CreateFacultyValidator()
        {
            RuleFor(x => x.name).NotEmpty();
        }
    }
}