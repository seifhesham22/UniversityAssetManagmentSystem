using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using Shared.Enums;
using UAMS.API.DTOs.Admin;
using UAMS.Campus.Features.AdminFeatures.CreateAssetManagerProfile;
using UAMS.Campus.Features.All.ListFacultiesQuery;
using UAMS.Campus.Features.DepartmentManagerFeatures.CreateManitainerProfile;
using UAMS.Campus.Features.DepartmentManagerFeatures.GetManitainerByDepartment;
using UAMS.Identity.Services.AuthService;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.DepartmentManagerOnly)]
    public class DepartmentManagerController(
        IAuthService _auth,
        IMediator mediator,
        CurrentUserFactory _currentUser) : ControllerBase
    {
        private Guid Me() => _currentUser.Create().UserId;

        [HttpPost("maintainers")]
        public async Task<IActionResult> CreateDepartmentManager(
        CreateMaintianerRequest req, CancellationToken ct)
        {
            var userId = await _auth.CreateUserAsync(req.email, req.password, Role.Maintainer, ct);
            try
            {
                var profileId = await mediator.Send(
                    new CreateMaintainerProfileCommand(Me(), req.fullName), ct);
                return Ok(new { UserId = userId, ProfileId = profileId });
            }
            catch
            {
                await _auth.DeleteUserAsync(userId, ct);
                throw;
            }
        }

        [HttpGet("maintainers/my")]
        public async Task<IActionResult> ListMaintainers(
            CancellationToken ct = default,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var res = await mediator.Send(new GetMaintainerByDepartmentCommand(Me(), page, pageSize), ct);
            return Ok(res);
        }
    }
}