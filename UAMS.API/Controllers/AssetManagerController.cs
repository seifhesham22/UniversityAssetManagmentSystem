using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using UAMS.Campus.Features.AssetManagerFeatures.AssignTeacherToFaculty;
using UAMS.Campus.Features.AssetManagerFeatures.GetAssetManagerProfile;
using UAMS.Campus.Features.AssetManagerFeatures.GetMyFacultyInfo;
using UAMS.Campus.Features.AssetManagerFeatures.GetMyFacultyTeachers;
using UAMS.Campus.Features.AssetManagerFeatures.GetStudentsOfMyFaculty;
using UAMS.Campus.Features.AssetManagerFeatures.RemoveTeacherFromFaculty;
using UAMS.Campus.Features.AssetManagerFeatures.SearchTeachers;
using UAMS.Room.Features.TicketFeatures.GetAmActionCount;
using UAMS.Room.Features.TicketFeatures.GetFacultyTickets;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.AssetManagerOnly)]
    public class AssetManagerController(IMediator mediator, CurrentUserFactory _currentUser) : ControllerBase
    {
        private Guid Me() => _currentUser.Create().UserId;

        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var res = await mediator.Send(new GetAssetManagerProfileCommand(Me()));
            return Ok(res);
        }

        [HttpGet("my-faculty")]
        public async Task<IActionResult> GetMyFacultyInfo(CancellationToken ct = default)
        {
            var res = await mediator.Send(new GetMyFacultyInfoQuery(Me()), ct);
            return Ok(res);
        }

        [HttpPost("teachers/{teacherId:guid}/faculties")]
        public async Task<IActionResult> AssignTeacherToFaculty(Guid teacherId, CancellationToken ct)
        {
            await mediator.Send(new AssignTeacherToFacultyCommand(teacherId, Me()), ct);
            return NoContent();
        }

        [HttpDelete("teachers/{teacherId:guid}/faculties")]
        public async Task<IActionResult> RemoveTeacherFromFaculty(Guid teacherId, CancellationToken ct)
        {
            await mediator.Send(new RemoveTeacherFromFacultyCommand(teacherId, Me()), ct);
            return NoContent();
        }

        [HttpGet("teachers/faculties")]
        public async Task<IActionResult> SearchTeachers(
            [FromQuery] string search,
            [FromQuery] bool unAssigned,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            return Ok(await mediator.Send(
                new SearchTeachersFacultyQuery(search, unAssigned, _currentUser.Create().facultyId, page, pageSize), ct));
        }

        [HttpGet("teachers/faculties/my")]
        public async Task<IActionResult> GetMyTeachers()
        {
            return Ok(await mediator.Send(new GetMyFacultyTeachersQueryCommand(Me())));
        }

        [HttpGet("student/my")]
        public async Task<IActionResult> StudentsByFaculty(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            return Ok(await mediator.Send(
                new GetStudentByFacultyCommand(Me(), page, pageSize), ct));
        }

        // ── Tickets ───────────────────────────────────────────────────────────
        [HttpGet("tickets")]
        public async Task<IActionResult> GetFacultyTickets(
            [FromQuery] bool needsAction = false,
            CancellationToken ct = default)
        {
            var facultyId = _currentUser.Create().facultyId;
            var result = await mediator.Send(new GetFacultyTicketsQuery(Me(), facultyId.Value, needsAction), ct);
            return Ok(result);
        }

        [HttpGet("tickets/action-count")]
        public async Task<IActionResult> GetActionCount(CancellationToken ct)
        {
            var facultyId = _currentUser.Create().facultyId;
            var count = await mediator.Send(new GetAmActionCountQuery(facultyId.Value), ct);
            return Ok(new { count });
        }
    }
}
