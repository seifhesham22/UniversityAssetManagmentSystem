using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using UAMS.Campus.Features.All.ListFacultiesQuery;
using UAMS.Campus.Presistence;
using UAMS.Room.Presistence;

namespace UAMS.Campus.Features.All.GetStudentFaculty
{
    public sealed record GetFacultyCommand(Guid facultyId) : IRequest<FullFacultyDto>;
    public sealed record FullFacultyDto(
        Guid Id,
        string name,
        int totalStudents,
        int totalTeachers);
    internal sealed class GetFullFacultyCommandHandler(CampusDbContext _db)
        : IRequestHandler<GetFacultyCommand, FullFacultyDto>
    {
        public async Task<FullFacultyDto> Handle(
            GetFacultyCommand request,
            CancellationToken cancellationToken)
        {
            var faculty = await _db
                .faculties
                .FirstOrDefaultAsync(x => x.Id == request.facultyId)
                ?? throw new InvalidOperationException("Couldn't find the faculty");

            var teachersCount = await _db.teacherFaculties
                .Where(x => x.FacultyId == request.facultyId)
                .CountAsync();

            return new FullFacultyDto(faculty.Id, faculty.Name, faculty.Students.Count(), teachersCount);
        }
    }
}