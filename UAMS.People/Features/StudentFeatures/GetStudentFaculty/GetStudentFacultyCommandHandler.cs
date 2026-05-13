using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Features.All.ListFacultiesQuery;
using UAMS.Campus.Presistence;
using UAMS.Room.Presistence;

namespace UAMS.Campus.Features.StudentFeatures.GetStudentFaculty
{
    public sealed record GetStudentFacultyCommand(Guid userId) : IRequest<FacultyDto>;
    internal sealed class GetStudentFacultyCommandHandler(CampusDbContext _db)
        : IRequestHandler<GetStudentFacultyCommand, FacultyDto>
    {
        public async Task<FacultyDto> Handle(GetStudentFacultyCommand request, CancellationToken cancellationToken)
        {
            var student = await _db.students
                .Include(x => x.Faculty)
                .FirstOrDefaultAsync(x => x.UserId == request.userId) 
                ?? throw new UnauthorizedAccessException("couldnt find a user with this Id");

            return new FacultyDto(student.FacultyId, student.Faculty.Name);
        }
    }
}