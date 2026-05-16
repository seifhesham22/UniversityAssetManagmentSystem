using MediatR;
using Microsoft.EntityFrameworkCore;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AssetManagerFeatures.GetMyFacultyInfo
{
    public sealed record GetMyFacultyInfoQuery(Guid UserId) : IRequest<FacultyInfoDto>;

    public sealed record FacultyInfoDto(
        Guid FacultyId,
        string FacultyName,
        int BuildingCount,
        int TeacherCount,
        int StudentCount);

    public sealed class GetMyFacultyInfoQueryHandler(CampusDbContext _db)
        : IRequestHandler<GetMyFacultyInfoQuery, FacultyInfoDto>
    {
        public async Task<FacultyInfoDto> Handle(
            GetMyFacultyInfoQuery request, CancellationToken cancellationToken)
        {
            var am = await _db.asset_managers
                .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken)
                ?? throw new UnauthorizedAccessException("Asset manager profile not found.");

            var faculty = await _db.faculties
                .FirstOrDefaultAsync(f => f.Id == am.FacultyId, cancellationToken)
                ?? throw new InvalidOperationException("Faculty not found.");

            var buildingCount = await _db.facultyBuildings
                .CountAsync(fb => fb.FacultyId == am.FacultyId, cancellationToken);

            var teacherCount = await _db.teacherFaculties
                .CountAsync(tf => tf.FacultyId == am.FacultyId, cancellationToken);

            var studentCount = await _db.students
                .CountAsync(s => s.FacultyId == am.FacultyId, cancellationToken);

            return new FacultyInfoDto(
                am.FacultyId, faculty.Name,
                buildingCount, teacherCount, studentCount);
        }
    }
}
