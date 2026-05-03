using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Enums;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Transactions;
using System.Xml.Linq;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.CreateStudentProfile
{
    public sealed record CreateStudentProfileCommand(
        Guid userId,
        string fullName,
        Guid facultyId)
        : IRequest<Guid>;
    public sealed class CreateStudentProfileCommandHandler(CampusDbContext _db)
        : IRequestHandler<CreateStudentProfileCommand, Guid>

    {
        public async Task<Guid> Handle(CreateStudentProfileCommand request, CancellationToken cancellationToken)
        {
            var student = await _db
                .students
                .FirstOrDefaultAsync(x => x.UserId == request.userId, cancellationToken);

            if (student != null)
                throw new InvalidOperationException("user alerady registered Нахуй");

            var faculty = await _db.faculties
                .FirstOrDefaultAsync(x => x.Id == request.facultyId) 
                ?? throw new InvalidOperationException($"couldn't find a faculty with the Id {request.facultyId}");

            var studentProfile = new Student(
                userId: request.userId,
                facultyId: request.facultyId,
                fullName: request.fullName);

            await _db.students.AddAsync(studentProfile);
            await _db.SaveChangesAsync();

            return studentProfile.Id;
        }
    }
}