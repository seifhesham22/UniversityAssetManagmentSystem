using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using UAMS.API.DTOs.Admin;
using UAMS.Campus.Features.AssetManagerFeatures.AssignTeacherToFaculty;
using UAMS.Campus.Features.AssetManagerFeatures.GetAssetManagerProfile;
using UAMS.Campus.Features.AssetManagerFeatures.GetMyFacultyTeachers;
using UAMS.Campus.Features.AssetManagerFeatures.GetStudentsOfMyFaculty;
using UAMS.Campus.Features.AssetManagerFeatures.RemoveTeacherFromFaculty;
using UAMS.Campus.Features.AssetManagerFeatures.SearchTeachers;

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

        [HttpPost("teachers/{teacherId:guid}/faculties")]
        public async Task<IActionResult> AssignTeacherToFaculty(
        Guid teacherId,
        CancellationToken ct)
        {
            await mediator.Send(new AssignTeacherToFacultyCommand(teacherId, Me()), ct);
            return NoContent();
        }

        [HttpDelete("teachers/{teacherId:guid}/faculties")]
        public async Task<IActionResult> RemoveTeacherFromFaculty(
        Guid teacherId,
        CancellationToken ct)
        {
            await mediator.Send(new RemoveTeacherFromFacultyCommand(teacherId, Me()), ct);
            return NoContent();
        }

        [HttpGet("teachers/faculties")]
        public async Task<IActionResult> SearchTeachers(
            [FromQuery]string search,
            [FromQuery]bool unAssigned,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            return Ok(await mediator
                .Send(new SearchTeachersFacultyQuery(
                    search,
                    unAssigned,
                    page,
                    pageSize)
                , ct));
        }

        [HttpGet("teachers/faculties/my")]
        public async Task<IActionResult> GetMyTeachers()
        {
            return Ok(await mediator
                .Send(new GetMyFacultyTeachersQueryCommand(Me())));
        }

        [HttpGet("student/my")]
        public async Task<IActionResult> StudentsByFaculty(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            return Ok(await mediator
                .Send(new GetStudentByFacultyCommand(
                Me(),
                page,
                pageSize),
                ct));
        }
    }
}