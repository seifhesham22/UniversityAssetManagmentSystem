using FluentValidation;

namespace UAMS.Campus.Features.CreateTeacherProfile
{
    public sealed class CreateTeacherProfileValidator
        : AbstractValidator<CreateTeacherProfileCommand>
    {
        public CreateTeacherProfileValidator()
        {
            RuleFor(x => x.userId).NotEmpty();
            RuleFor(x => x.fullName).NotEmpty().MaximumLength(60);
        }
    }
}