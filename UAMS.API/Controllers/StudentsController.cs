using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using UAMS.Campus.Features.AssetManagerFeatures.GetStudentsOfMyFaculty;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController(IMediator mediator, CurrentUserFactory _user) : ControllerBase
    {
        [HttpGet("my-faculty")]
        [Authorize(Policy = Policies.StudentOnly)]
        public async Task<IActionResult> GetStudentFaculty()
        {
            var user = _user.Create().UserId;
            var res = await mediator.Send(new GetStudentByFacultyCommand(user));
            return Ok(res);
        }
    }
}