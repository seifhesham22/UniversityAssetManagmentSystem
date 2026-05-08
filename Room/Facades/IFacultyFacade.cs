using System;
using System.Collections.Generic;
using System.Text;

namespace UAMS.Room.Facades
{
    public interface IFacultyFacade
    {
        Task<bool> ExistsAsync(Guid facultyId, CancellationToken ct);
        Task<bool> IsBuildingLinkedAsync(Guid facultyId, Guid buildingId, CancellationToken ct);
    }
}