using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.TeacherFeatures.CreateTeacherProfile
{
    public sealed record CreateTeacherProfileCommand(Guid userId, string fullName) : IRequest<Guid>;
    public sealed class CreateTeacherProfileCommandHandler(CampusDbContext _db)
        : IRequestHandler<CreateTeacherProfileCommand, Guid>
    {
        public async Task<Guid> Handle(CreateTeacherProfileCommand request, CancellationToken cancellationToken)
        {
            var teacher = await _db.teachers
                .FirstOrDefaultAsync(t => t.UserId == request.userId, cancellationToken);

            if (teacher is not null)
                throw new InvalidOperationException("teacher already exists");

            var teacherProfile = new Teacher(
                userId: request.userId,
                fullName: request.fullName);

            await _db.teachers.AddAsync(teacherProfile);
            await _db.SaveChangesAsync();

            return teacherProfile.Id;
        }
    }
}