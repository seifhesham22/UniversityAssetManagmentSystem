using Microsoft.EntityFrameworkCore;
using UAMS.Campus.Presistence;
using UAMS.Room.Facades;

namespace UAMS.Campus.FacadeImplementations
{
    public sealed class CampusFacade(CampusDbContext _db) : ICampusFacade
    {
        public async Task<Dictionary<Guid, string>> GetUserNamesAsync(
            IEnumerable<Guid> userIds, CancellationToken ct)
        {
            var ids = userIds.ToHashSet();
            var result = new Dictionary<Guid, string>();

            var students = await _db.students
                .Where(s => ids.Contains(s.UserId))
                .Select(s => new { s.UserId, s.FullName })
                .ToListAsync(ct);

            var teachers = await _db.teachers
                .Where(t => ids.Contains(t.UserId))
                .Select(t => new { t.UserId, t.FullName })
                .ToListAsync(ct);

            var managers = await _db.asset_managers
                .Where(a => ids.Contains(a.UserId))
                .Select(a => new { a.UserId, a.FullName })
                .ToListAsync(ct);

            var deptManagers = await _db.department_managers
                .Where(d => ids.Contains(d.UserId))
                .Select(d => new { d.UserId, d.FullName })
                .ToListAsync(ct);

            var maintainers = await _db.maintainers
                .Where(m => ids.Contains(m.UserId))
                .Select(m => new { m.UserId, m.FullName })
                .ToListAsync(ct);

            foreach (var x in students) result.TryAdd(x.UserId, x.FullName);
            foreach (var x in teachers) result.TryAdd(x.UserId, x.FullName);
            foreach (var x in managers) result.TryAdd(x.UserId, x.FullName);
            foreach (var x in deptManagers) result.TryAdd(x.UserId, x.FullName);
            foreach (var x in maintainers) result.TryAdd(x.UserId, x.FullName);

            return result;
        }

        public async Task<string?> GetFacultyNameAsync(Guid facultyId, CancellationToken ct)
            => await _db.faculties
                .Where(f => f.Id == facultyId)
                .Select(f => f.Name)
                .FirstOrDefaultAsync(ct);

        public async Task<string?> GetBuildingNameAsync(Guid buildingId, CancellationToken ct)
            => await _db.buildings
                .Where(b => b.Id == buildingId)
                .Select(b => b.Name)
                .FirstOrDefaultAsync(ct);

        public async Task<string?> GetDepartmentNameAsync(Guid departmentId, CancellationToken ct)
            => await _db.departments
                .Where(d => d.Id == departmentId)
                .Select(d => d.Name)
                .FirstOrDefaultAsync(ct);

        public async Task<string?> GetMaintainerNameAsync(Guid maintainerId, CancellationToken ct)
            => await _db.maintainers
                .Where(m => m.Id == maintainerId)
                .Select(m => m.FullName)
                .FirstOrDefaultAsync(ct);

        public async Task<Guid?> GetMaintainerIdByUserIdAsync(Guid userId, CancellationToken ct)
            => await _db.maintainers
                .Where(m => m.UserId == userId)
                .Select(m => (Guid?)m.Id)
                .FirstOrDefaultAsync(ct);

        public async Task<Guid?> GetDepartmentManagerDepartmentIdAsync(Guid userId, CancellationToken ct)
        {
            var dm = await _db.department_managers
                .FirstOrDefaultAsync(d => d.UserId == userId, ct);
            if (dm == null || dm.DepartmentId == Guid.Empty) return null;
            return dm.DepartmentId;
        }

        public async Task<bool> IsMaintainerInDepartmentAsync(
            Guid maintainerId, Guid departmentId, CancellationToken ct)
            => await _db.maintainers
                .AnyAsync(m => m.Id == maintainerId && m.DepartmentId == departmentId, ct);

        public async Task<bool> DepartmentExistsAsync(Guid departmentId, CancellationToken ct)
            => await _db.departments.AnyAsync(d => d.Id == departmentId, ct);

        public async Task<string?> GetMaintainerVkIdAsync(Guid maintainerId, CancellationToken ct)
            => await _db.maintainers
                .Where(m => m.Id == maintainerId)
                .Select(m => m.VkId)
                .FirstOrDefaultAsync(ct);

        public async Task<Guid?> GetMaintainerIdByVkIdAsync(string vkId, CancellationToken ct)
            => await _db.maintainers
                .Where(m => m.VkId == vkId)
                .Select(m => (Guid?)m.Id)
                .FirstOrDefaultAsync(ct);

        public async Task<string?> GetAssetManagerNameByFacultyIdAsync(Guid facultyId, CancellationToken ct)
            => await _db.asset_managers
                .Where(a => a.FacultyId == facultyId)
                .Select(a => a.FullName)
                .FirstOrDefaultAsync(ct);

        public async Task<(string Name, string? Address)?> GetBuildingInfoAsync(Guid buildingId, CancellationToken ct)
        {
            var b = await _db.buildings
                .Where(b => b.Id == buildingId)
                .Select(b => new { b.Name, b.Address })
                .FirstOrDefaultAsync(ct);
            return b == null ? null : (b.Name, b.Address);
        }

        public async Task<Dictionary<Guid, string>> GetNoteAuthorNamesAsync(
            IEnumerable<Guid> authorIds, CancellationToken ct)
        {
            var ids    = authorIds.ToHashSet();
            var result = new Dictionary<Guid, string>();

            // Look up by UserId (asset managers, dept managers, students, teachers)
            var byUser = await GetUserNamesAsync(ids, ct);
            foreach (var kv in byUser) result.TryAdd(kv.Key, kv.Value);

            // Look up remaining by maintainer profile Id
            var unresolved = ids.Where(id => !result.ContainsKey(id)).ToList();
            if (unresolved.Count > 0)
            {
                var maintainerNames = await _db.maintainers
                    .Where(m => unresolved.Contains(m.Id))
                    .Select(m => new { m.Id, m.FullName })
                    .ToListAsync(ct);
                foreach (var m in maintainerNames) result.TryAdd(m.Id, m.FullName);
            }

            return result;
        }
    }
}
