using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using Shared.Enums;
using UAMS.API.DTOs.Admin;
using UAMS.API.DTOs.Auth;
using UAMS.Campus.Features.TeacherFeatures.CreateTeacherProfile;
using UAMS.Campus.Features.TeacherFeatures.GetTeacherFaculties;
using UAMS.Identity.Services.AuthService;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.TeacherOnly)]
    public class TeacherController(
        IMediator _mediator,
        CurrentUserFactory _user) : ControllerBase
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