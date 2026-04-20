using MediatR;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.GetStudentByFaculty
{
    public sealed record GetStudentByFacultyCommand(
        Guid facultyId,
        int page = 1,
        int pageSize = 20)
        : IRequest<PagedResult<StudentDto>>;

    public sealed record StudentDto(Guid Id, string fullName);
    public sealed class GetStudentByFacultyCommandHandler(CampusDbContext _db)
        : IRequestHandler<GetStudentByFacultyCommand, PagedResult<StudentDto>>
    {
        public async Task<PagedResult<StudentDto>> Handle(GetStudentByFacultyCommand request, CancellationToken cancellationToken)
        {
            var q =  _db.students.AsQueryable().AsNoTracking();

            q = q.Where(x => x.FacultyId == request.facultyId);

            var total = await q.CountAsync(cancellationToken);
            var items = await q.Skip((1 - request.page) * request.pageSize)
                .Take(request.pageSize)
                .Select(s => new StudentDto(s.Id, s.FullName))
                .ToListAsync();

            return new PagedResult<StudentDto>(items, total, request.page, request.pageSize);
        }
    }
}