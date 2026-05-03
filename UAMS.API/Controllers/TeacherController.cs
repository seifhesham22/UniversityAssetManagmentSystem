using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using UAMS.Campus.Features.GetTeacherFaculties;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.TeacherOnly)]
    public class TeacherController(IMediator _mediator, CurrentUserFactory _user) : ControllerBase
    {
        [HttpGet("my-faculties")]
        public async Task<IActionResult> MyFaculties()
        {
            var user =  _user.Create();
            var res = await _mediator.Send(new GetTeacherFacultiesCommand(user.UserId));
            return Ok(res);
        }
    }
}
