using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using UAMS.Room.Features.TicketFeatures.AssignMaintainer;
using UAMS.Room.Features.TicketFeatures.CloseTicket;
using UAMS.Room.Features.TicketFeatures.ConfirmFixed;
using UAMS.Room.Features.TicketFeatures.CreateTicket;
using UAMS.Room.Features.TicketFeatures.EscalateExternally;
using UAMS.Room.Features.TicketFeatures.GetTicketDetail;
using UAMS.Room.Features.TicketFeatures.MarkFixed;
using UAMS.Room.Features.TicketFeatures.MarkReplaced;
using UAMS.Room.Features.TicketFeatures.ReportIrreparable;
using UAMS.Room.Features.TicketFeatures.ReportNeedsParts;
using UAMS.Room.Features.TicketFeatures.SendForFix;
using UAMS.Room.Features.TicketFeatures.SendForInspection;
using UAMS.Room.Features.TicketFeatures.SendForReplacement;
using UAMS.Room.Features.TicketFeatures.ClaimTicket;
using UAMS.Room.Features.TicketFeatures.ReassignMaintainer;
using UAMS.Room.Features.TicketFeatures.SubmitInspectionResult;

namespace UAMS.API.Controllers
{
    [ApiController]
    [Route("api/tickets")]
    [Authorize]
    public sealed class TicketController(
        IMediator _mediator,
        CurrentUserFactory _currentUser) : ControllerBase
    {
        private Guid Me() => _currentUser.Create().UserId;

        // ── Report a ticket (Student or Teacher) ──────────────────────────────
        public sealed record ReportTicketRequest(
            Guid PlacedAssetId,
            Guid RoomId,
            Guid FacultyId,
            string? Description);

        [HttpPost]
        [Authorize(Policy = Policies.CanReportIssue)]
        public async Task<IActionResult> ReportTicket(ReportTicketRequest req, CancellationToken ct)
        {
            var id = await _mediator.Send(
                new ReportTicketCommand(req.PlacedAssetId, req.RoomId, req.FacultyId, Me(), req.Description), ct);
            return Ok(new { Id = id });
        }

        // ── Ticket detail (any authorised user) ───────────────────────────────
        [HttpGet("{ticketId:guid}")]
        public async Task<IActionResult> GetTicketDetail(Guid ticketId, CancellationToken ct)
        {
            var result = await _mediator.Send(new GetTicketDetailQuery(ticketId), ct);
            return Ok(result);
        }

        // ── Asset Manager transitions ─────────────────────────────────────────
        public sealed record SendRequest(Guid DepartmentId, string? Note);

        [HttpPost("{ticketId:guid}/send-for-inspection")]
        [Authorize(Policy = Policies.CanManageTicket)]
        public async Task<IActionResult> SendForInspection(Guid ticketId, SendRequest req, CancellationToken ct)
        {
            await _mediator.Send(new SendForInspectionCommand(ticketId, Me(), req.DepartmentId, req.Note), ct);
            return NoContent();
        }

        [HttpPost("{ticketId:guid}/send-for-fix")]
        [Authorize(Policy = Policies.CanManageTicket)]
        public async Task<IActionResult> SendForFix(Guid ticketId, SendRequest req, CancellationToken ct)
        {
            await _mediator.Send(new SendForFixCommand(ticketId, Me(), req.DepartmentId, req.Note), ct);
            return NoContent();
        }

        [HttpPost("{ticketId:guid}/send-for-replacement")]
        [Authorize(Policy = Policies.CanManageTicket)]
        public async Task<IActionResult> SendForReplacement(Guid ticketId, SendRequest req, CancellationToken ct)
        {
            await _mediator.Send(new SendForReplacementCommand(ticketId, Me(), req.DepartmentId, req.Note), ct);
            return NoContent();
        }

        public sealed record EscalateRequest(string? Note);

        [HttpPost("{ticketId:guid}/escalate")]
        [Authorize(Policy = Policies.CanManageTicket)]
        public async Task<IActionResult> Escalate(Guid ticketId, EscalateRequest req, CancellationToken ct)
        {
            await _mediator.Send(new EscalateExternallyCommand(ticketId, Me(), req.Note), ct);
            return NoContent();
        }

        public sealed record CloseRequest(string? Note);

        [HttpPost("{ticketId:guid}/close")]
        [Authorize(Policy = Policies.CanManageTicket)]
        public async Task<IActionResult> Close(Guid ticketId, CloseRequest req, CancellationToken ct)
        {
            await _mediator.Send(new CloseTicketCommand(ticketId, Me(), req.Note), ct);
            return NoContent();
        }

        // ── Confirm fix (Teacher or Asset Manager) ────────────────────────────
        [HttpPost("{ticketId:guid}/confirm-fix")]
        [Authorize(Policy = Policies.CanConfirmFix)]
        public async Task<IActionResult> ConfirmFix(Guid ticketId, CancellationToken ct)
        {
            await _mediator.Send(new ConfirmFixedCommand(ticketId, Me()), ct);
            return NoContent();
        }

        // ── Department Manager: assign maintainer ─────────────────────────────
        public sealed record AssignMaintainerRequest(Guid MaintainerId);

        [HttpPost("{ticketId:guid}/assign-maintainer")]
        [Authorize(Policy = Policies.DepartmentManagerOnly)]
        public async Task<IActionResult> AssignMaintainer(
            Guid ticketId, AssignMaintainerRequest req, CancellationToken ct)
        {
            await _mediator.Send(new AssignMaintainerCommand(ticketId, Me(), req.MaintainerId), ct);
            return NoContent();
        }

        // ── Maintainer transitions ────────────────────────────────────────────
        [HttpPost("{ticketId:guid}/mark-fixed")]
        [Authorize(Policy = Policies.MaintainerOnly)]
        public async Task<IActionResult> MarkFixed(Guid ticketId, CancellationToken ct)
        {
            await _mediator.Send(new MarkFixedCommand(ticketId, Me()), ct);
            return NoContent();
        }

        [HttpPost("{ticketId:guid}/mark-replaced")]
        [Authorize(Policy = Policies.MaintainerOnly)]
        public async Task<IActionResult> MarkReplaced(Guid ticketId, CancellationToken ct)
        {
            await _mediator.Send(new MarkReplacedCommand(ticketId, Me()), ct);
            return NoContent();
        }

        public sealed record SubmitInspectionRequest(bool IsRepairable, string Notes);

        [HttpPost("{ticketId:guid}/submit-inspection")]
        [Authorize(Policy = Policies.CanSubmitInspection)]
        public async Task<IActionResult> SubmitInspection(
            Guid ticketId, SubmitInspectionRequest req, CancellationToken ct)
        {
            await _mediator.Send(
                new SubmitInspectionResultCommand(ticketId, Me(), req.IsRepairable, req.Notes), ct);
            return NoContent();
        }

        public sealed record ReasonRequest(string Reason);

        [HttpPost("{ticketId:guid}/report-needs-parts")]
        [Authorize(Policy = Policies.MaintainerOnly)]
        public async Task<IActionResult> ReportNeedsParts(
            Guid ticketId, ReasonRequest req, CancellationToken ct)
        {
            await _mediator.Send(new ReportNeedsPartsCommand(ticketId, Me(), req.Reason), ct);
            return NoContent();
        }

        [HttpPost("{ticketId:guid}/report-irreparable")]
        [Authorize(Policy = Policies.MaintainerOnly)]
        public async Task<IActionResult> ReportIrreparable(
            Guid ticketId, ReasonRequest req, CancellationToken ct)
        {
            await _mediator.Send(new ReportIrreparableCommand(ticketId, Me(), req.Reason), ct);
            return NoContent();
        }

        // ── Department Manager: reassign maintainer ───────────────────────────
        public sealed record ReassignMaintainerRequest(Guid NewMaintainerId);

        [HttpPost("{ticketId:guid}/reassign-maintainer")]
        [Authorize(Policy = Policies.DepartmentManagerOnly)]
        public async Task<IActionResult> ReassignMaintainer(
            Guid ticketId, ReassignMaintainerRequest req, CancellationToken ct)
        {
            await _mediator.Send(new ReassignMaintainerCommand(ticketId, Me(), req.NewMaintainerId), ct);
            return NoContent();
        }

        // ── Maintainer: claim an unassigned ticket ───────────────────────────
        [HttpPost("{ticketId:guid}/claim")]
        [Authorize(Policy = Policies.MaintainerOnly)]
        public async Task<IActionResult> Claim(Guid ticketId, CancellationToken ct)
        {
            await _mediator.Send(new ClaimTicketCommand(ticketId, Me()), ct);
            return NoContent();
        }
    }
}
