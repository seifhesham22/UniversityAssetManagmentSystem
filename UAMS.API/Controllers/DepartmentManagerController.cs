using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using Shared.Enums;
using UAMS.Campus.Features.DepartmentManagerFeatures.CreateManitainerProfile;
using UAMS.Campus.Features.DepartmentManagerFeatures.GetManitainerByDepartment;
using UAMS.Identity.Services.AuthService;
using UAMS.API.DTOs.Admin;
using UAMS.Campus.Features.DepartmentManagerFeatures.RemoveMaintainer;
using UAMS.Room.Features.TicketFeatures.GetDepartmentTickets;
using UAMS.Room.Features.TicketFeatures.GetDeptActionCount;
using UAMS.Room.Features.TicketFeatures.RetryVkNotification;

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
        public async Task<IActionResult> CreateMaintainer(CreateMaintianerRequest req, CancellationToken ct)
        {
            var userId = await _auth.CreateUserAsync(req.email, req.password, Role.Maintainer, ct);
            try
            {
                var profileId = await mediator.Send(
                    new CreateMaintainerProfileCommand(Me(), userId, req.fullName, req.vkId), ct);
                return Ok(new { UserId = userId, ProfileId = profileId });
            }
            catch
            {
                await _auth.DeleteUserAsync(userId, ct);
                throw;
            }
        }

        [HttpDelete("maintainers/{id:guid}")]
        public async Task<IActionResult> RemoveMaintainer(Guid id, CancellationToken ct)
        {
            var userId = await mediator.Send(new RemoveMaintainerCommand(Me(), id), ct);
            await _auth.DeleteUserAsync(userId, ct);
            return NoContent();
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

        // ── Tickets ───────────────────────────────────────────────────────────
        [HttpGet("tickets")]
        public async Task<IActionResult> GetDepartmentTickets(
            [FromQuery] bool needsAction = false,
            CancellationToken ct = default)
        {
            var result = await mediator.Send(new GetDepartmentTicketsQuery(Me(), needsAction), ct);
            return Ok(result);
        }

        [HttpGet("tickets/action-count")]
        public async Task<IActionResult> GetActionCount(CancellationToken ct)
        {
            var count = await mediator.Send(new GetDeptActionCountQuery(Me()), ct);
            return Ok(new { count });
        }

        [HttpPost("tickets/{id:guid}/resend-notification")]
        public async Task<IActionResult> ResendVkNotification(Guid id, CancellationToken ct)
        {
            var sent = await mediator.Send(new RetryVkNotificationCommand(id, Me()), ct);
            return Ok(new { sent });
        }
    }
}
