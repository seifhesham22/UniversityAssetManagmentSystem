using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using Shared.Enums;
using UAMS.Campus.Features.AdminFeatures.ListBuildingsQuery;
using UAMS.Campus.Features.All.GetBuildingsAssociatedToFaculty;
using UAMS.Campus.Features.All.ListDepartments;
using UAMS.Campus.Features.All.ListFacultiesQuery;
using UAMS.Room.Features.RoomManagment.GetRoomsByFaculty;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.CanViewFaculties)]
    public class CampusController(
        IMediator mediator,
        CurrentUserFactory _currentUser) : ControllerBase
    {
        private Guid Me() => _currentUser.Create().UserId;

        [AllowAnonymous]
        [HttpGet("faculties")]
        public async Task<IActionResult> ListFaculties(
            [FromQuery] string? search,
            CancellationToken ct = default,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var res = await mediator.Send(new ListFacultiesQuery(search, page, pageSize), ct);
            return Ok(res);
        }

        [AllowAnonymous]
        [HttpGet("departments")]
        public async Task<IActionResult> ListDepartments(
            [FromQuery] string? search,
            [FromQuery] AssetCategory? Category = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await mediator.Send(new ListDepartmentCommand(search, Category, page, pageSize), ct);
            return Ok(res);
        }
        [HttpGet("faculty/buildings/{facultyId:guid}")]
        public async Task<IActionResult> ListFacultyBuildings(Guid facultyId)
        {
            var res = await mediator.Send(new GetFacultyBuildingQueryCommand(facultyId));
            return Ok(res);
        }

        [HttpGet("faculty/room/{facultyId:guid}")]
        public async Task<IActionResult> GetFacultyRooms(
            Guid facultyId,
            [FromQuery] int page,
            [FromQuery] int totalSize)
        {
            return Ok(await mediator.Send(new GetRoomsByFacultyQueryCommand(
                Me(),
                facultyId,
                page,
                totalSize)));
        }
    }
}