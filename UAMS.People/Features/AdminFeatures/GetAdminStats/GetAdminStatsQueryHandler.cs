using MediatR;
using Microsoft.EntityFrameworkCore;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.GetAdminStats
{
    public sealed record GetAdminStatsQuery() : IRequest<AdminStatsDto>;

    public sealed record AdminStatsDto(int FacultyCount, int BuildingCount, int DepartmentCount);

    public sealed class GetAdminStatsQueryHandler(CampusDbContext _db)
        : IRequestHandler<GetAdminStatsQuery, AdminStatsDto>
    {
        public async Task<AdminStatsDto> Handle(
            GetAdminStatsQuery request,
            CancellationToken cancellationToken)
        {
            var facultyCount    = await _db.faculties.CountAsync(cancellationToken);
            var buildingCount   = await _db.buildings.CountAsync(cancellationToken);
            var departmentCount = await _db.departments.CountAsync(cancellationToken);

            return new AdminStatsDto(facultyCount, buildingCount, departmentCount);
        }
    }
}
