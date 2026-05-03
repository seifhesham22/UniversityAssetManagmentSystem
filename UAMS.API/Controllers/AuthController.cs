using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using UAMS.API.DTOs.Auth;
using UAMS.Campus.Features.CreateStudentProfile;
using UAMS.Campus.Features.CreateTeacherProfile;
using UAMS.Identity.Services.AuthService;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(
        IAuthService _authService,
        IMediator mediator) : ControllerBase
    {
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequestForAll request, CancellationToken ct)
        {
            var res = await _authService.LoginAsync(request.email, request.password, ct);
            return Ok(res);
        }
        [AllowAnonymous]
        [HttpPost("register/teacher")]
        public async Task<IActionResult> RegisterTeacher(RegisterTeacherRequest request, CancellationToken ct)
        {
            Guid userId;
            try
            {
                userId = await _authService.CreateUserAsync(
                    request.email,
                    request.password,
                    Shared.Enums.Role.Teacher,
                    ct);
            }
            catch { throw; }

            Guid profileId;
            try
            {
                profileId = await mediator.Send(new CreateTeacherProfileCommand(userId, request.fullName));
            }
            catch
            {
                await _authService.DeleteUserAsync(userId, ct);
                throw;
            }

            return Ok(profileId);
        }
        [AllowAnonymous]
        [HttpPost("register/student")]
        public async Task<IActionResult> Registerstudent(RegisterStudentRequest request, CancellationToken ct)
        {
            Guid userId;
            try
            {
                userId = await _authService.CreateUserAsync(
                    request.email,
                    request.password,
                    Shared.Enums.Role.Student,
                    ct);
            }
            catch { throw; }

            Guid profileId;
            try
            {
                profileId = await mediator.Send(new CreateStudentProfileCommand(userId, request.fullName, request.facultyId));
            }
            catch
            {
                await _authService.DeleteUserAsync(userId, ct);
                throw;
            }

            return Ok(profileId);
        }
    }
}