using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using UAMS.API.DTOs.Admin;
using UAMS.Campus.Features.AssignTeacherToFaculty;
using UAMS.Campus.Features.GetStudentByFaculty;
using UAMS.Campus.Features.GetTeacherByFaculty;
using UAMS.Campus.Features.RemoveTeacherFromFaculty;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.AssetManagerOnly)]
    public class AssetManagerController(IMediator mediator) : ControllerBase
    {
        [HttpPost("teachers/{teacherId:guid}/faculties")]
        public async Task<IActionResult> AssignFaculty(
        Guid teacherId, [FromBody] AssignFacultyRequest body, CancellationToken ct)
        {
            await mediator.Send(new AssignTeacherToFacultyCommand(teacherId, body.facultyId), ct);
            return NoContent();
        }

        [HttpDelete("teachers/{teacherId:guid}/faculties/{facultyId:guid}")]
        public async Task<IActionResult> RemoveFaculty(
        Guid teacherId, Guid facultyId, CancellationToken ct)
        {
            await mediator.Send(new RemoveTeacherFromFacultyCommand(teacherId, facultyId), ct);
            return NoContent();
        }

        [HttpGet("faculties/{facultyId:guid}/teachers")]
        public async Task<IActionResult> TeachersByFaculty(
            Guid facultyId, [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => Ok(await mediator.Send(new GetTeacherByFacultyQuery(facultyId, page, pageSize), ct));

        [HttpGet("faculties/{facultyId:guid}/students")]
        public async Task<IActionResult> StudentsByFaculty(
            Guid facultyId, [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20, CancellationToken ct = default)
            => Ok(await mediator.Send(new GetStudentByFacultyCommand(facultyId, page, pageSize), ct));
    }
}