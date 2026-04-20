using FluentValidation;

namespace UAMS.Campus.Features.CreateStudentProfile
{
    public sealed class CreateStudentProfileValidator : AbstractValidator<CreateStudentProfileCommand>
    {
        public CreateStudentProfileValidator()
        {
            RuleFor(x => x.userId).NotEmpty();
            RuleFor(x => x.facultyId).NotEmpty();
            RuleFor(x => x.fullName).NotEmpty().MaximumLength(60);
        }
    }
}