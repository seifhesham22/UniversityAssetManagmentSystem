using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using Shared.Enums;
using UAMS.Room.Features.AssetDefinitionFeatures.AddCheckListItemToAsset;
using UAMS.Room.Features.AssetDefinitionFeatures.CreateAssetDefinition;
using UAMS.Room.Features.AssetDefinitionFeatures.DeleteAssetDefinition;
using UAMS.Room.Features.AssetDefinitionFeatures.GetAssetDefinitionById;
using UAMS.Room.Features.AssetDefinitionFeatures.GetAssetDefinitionsWithPlacementRulesQuery;
using UAMS.Room.Features.AssetDefinitionFeatures.ListAssetDefinitionsQuery;
using UAMS.Room.Features.AssetDefinitionFeatures.RemoveCheckListItemFromAsset;
using UAMS.Room.Features.AssetDefinitionFeatures.UpdateAssetDefinition;
using UAMS.Room.Features.LayoutFeatures;
using UAMS.Room.Features.LayoutFeatures.GetChecklistForPlacedAsset;
using UAMS.Room.Features.LayoutFeatures.UpdateChecklist;
using UAMS.Room.Features.RoomManagment.CloseRoom;
using UAMS.Room.Features.RoomManagment.CreateRoom;
using UAMS.Room.Features.RoomManagment.GetRoomById;
using UAMS.Room.Features.RoomManagment.GetRoomsByFaculty;
using UAMS.Room.Features.RoomManagment.ReOpenRoom;

namespace UAMS.API.Controllers
{
    [ApiController]
    [Route("api/room-design")]
    [Authorize]
    public sealed class RoomDesignController(
        IMediator mediator,
        CurrentUserFactory currentUser) : ControllerBase
    {
        private Guid Me() => currentUser.Create().UserId;

        // ══════════════════════════════════════════════════════════════════════
        // Asset Definitions
        // ══════════════════════════════════════════════════════════════════════

        // GET  /api/room-design/assets            (Admin + AssetManager)
        [HttpGet("assets")]
        [Authorize(Policy = Policies.CanManageAssets)]
        public async Task<IActionResult> ListAssets(
            [FromQuery] string? search,
            [FromQuery] AssetCategory? category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var result = await mediator.Send(
                new ListAssetDefinitionsQueryCommand(search, category, page, pageSize), ct);
            return Ok(result);
        }

        // GET  /api/room-design/assets/placement-rules   (CanDesignRoom – used by the canvas)
        [HttpGet("assets/placement-rules")]
        [Authorize(Policy = Policies.CanDesignRoom)]
        public async Task<IActionResult> GetPlacementRules(CancellationToken ct)
        {
            var result = await mediator.Send(new GetAssetDefinitionsWithPlacementRulesCommand(), ct);
            return Ok(result);
        }

        // GET  /api/room-design/assets/{id}       (Admin + AssetManager)
        [HttpGet("assets/{id:guid}")]
        [Authorize(Policy = Policies.CanManageAssets)]
        public async Task<IActionResult> GetAsset(Guid id, CancellationToken ct)
        {
            var result = await mediator.Send(new GetAssetDefinitionByIdCommand(id), ct);
            return Ok(result);
        }

        // POST /api/room-design/assets            (Admin only)
        [HttpPost("assets")]
        [Authorize(Policy = Policies.SuperAdminOnly)]
        public async Task<IActionResult> CreateAsset(
            [FromBody] CreateAssetDefinitionCommand command, CancellationToken ct)
        {
            return Ok(new { Id = await mediator.Send(command, ct) });
        }

        // PUT  /api/room-design/assets/{id}       (Admin only)
        public sealed record UpdateAssetRequest(
            string Name,
            string SvgUrl,
            AssetCategory Category,
            List<PlacementLocation> Locations);

        [HttpPut("assets/{id:guid}")]
        [Authorize(Policy = Policies.SuperAdminOnly)]
        public async Task<IActionResult> UpdateAsset(
            Guid id, [FromBody] UpdateAssetRequest req, CancellationToken ct)
        {
            await mediator.Send(
                new UpdateAssetDefinitionCommand(id, req.Name, req.SvgUrl, req.Category, req.Locations), ct);
            return NoContent();
        }

        // DELETE /api/room-design/assets/{id}     (Admin only)
        [HttpDelete("assets/{id:guid}")]
        [Authorize(Policy = Policies.SuperAdminOnly)]
        public async Task<IActionResult> DeleteAsset(Guid id, CancellationToken ct)
        {
            await mediator.Send(new DeleteAssetDefinitionCommand(id), ct);
            return NoContent();
        }

        // POST /api/room-design/assets/{id}/checklist-items   (Admin only)
        public sealed record AddChecklistItemRequest(string Description);

        [HttpPost("assets/{id:guid}/checklist-items")]
        [Authorize(Policy = Policies.SuperAdminOnly)]
        public async Task<IActionResult> AddChecklistItem(
            Guid id, [FromBody] AddChecklistItemRequest req, CancellationToken ct)
        {
            var itemId = await mediator.Send(
                new AddCheckListItemToAssetCommand(id, req.Description), ct);
            return Ok(new { Id = itemId });
        }

        // DELETE /api/room-design/assets/{id}/checklist-items/{itemId}  (Admin only)
        [HttpDelete("assets/{id:guid}/checklist-items/{itemId:guid}")]
        [Authorize(Policy = Policies.SuperAdminOnly)]
        public async Task<IActionResult> RemoveChecklistItem(
            Guid id, Guid itemId, CancellationToken ct)
        {
            await mediator.Send(
                new RemoveAssetCheckListItemFromAssetDefinitionCommand(id, itemId), ct);
            return NoContent();
        }

        // ══════════════════════════════════════════════════════════════════════
        // Rooms
        // ══════════════════════════════════════════════════════════════════════

        public sealed record CreateRoomRequest(Guid facultyId, Guid buildingId, string name);

        [HttpPost("rooms")]
        [Authorize(Policy = Policies.CanDesignRoom)]
        public async Task<IActionResult> CreateRoom(CreateRoomRequest req, CancellationToken ct)
        {
            return Ok(await mediator.Send(
                new CreateRoomCommand(req.facultyId, req.buildingId, Me(), req.name), ct));
        }

        [HttpGet("rooms/{roomId:guid}")]
        [Authorize(Policy = Policies.CanViewRoomDesign)]
        public async Task<IActionResult> GetRoom(Guid roomId, CancellationToken ct)
        {
            return Ok(await mediator.Send(new GetRoomByIdQueryCommand(roomId), ct));
        }

        [HttpGet("rooms/faculty/{facultyId:guid}")]
        [Authorize(Policy = Policies.CanViewRoomDesign)]
        public async Task<IActionResult> GetRoomsByFaculty(
            Guid facultyId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            return Ok(await mediator.Send(
                new GetRoomsByFacultyQueryCommand(Me(), facultyId, page, pageSize), ct));
        }

        [HttpPost("rooms/{roomId:guid}/close")]
        [Authorize(Policy = Policies.CanDesignRoom)]
        public async Task<IActionResult> CloseRoom(
            Guid roomId, [FromBody] string? reason, CancellationToken ct)
        {
            await mediator.Send(new CloseRoomCommand(Me(), roomId, reason), ct);
            return NoContent();
        }

        [HttpPost("rooms/{roomId:guid}/reopen")]
        [Authorize(Policy = Policies.CanDesignRoom)]
        public async Task<IActionResult> ReopenRoom(Guid roomId, CancellationToken ct)
        {
            await mediator.Send(new ReopenRoomCommand(Me(), roomId), ct);
            return NoContent();
        }

        // ══════════════════════════════════════════════════════════════════════
        // Layout
        // ══════════════════════════════════════════════════════════════════════

        [HttpPut("rooms/{roomId:guid}/layout")]
        [Authorize(Policy = Policies.CanDesignRoom)]
        public async Task<IActionResult> SaveLayout(
            Guid roomId,
            [FromBody] List<PlacedAssetDto> placedAssets,
            CancellationToken ct)
        {
            await mediator.Send(new SaveLayoutCommand(roomId, Me(), placedAssets), ct);
            return NoContent();
        }

        // ══════════════════════════════════════════════════════════════════════
        // Checklists
        // ══════════════════════════════════════════════════════════════════════

        [HttpGet("placed-assets/{placedAssetId:guid}/checklist")]
        [Authorize(Policy = Policies.CanCheckChecklist)]
        public async Task<IActionResult> GetChecklist(
            Guid placedAssetId,
            [FromQuery] string? studyYear,
            CancellationToken ct)
        {
            return Ok(await mediator.Send(
                new GetChecklistForPlacedAssetQuery(placedAssetId, studyYear), ct));
        }

        public sealed record UpdateChecklistRequest(Guid ChecklistItemId, bool IsChecked);

        [HttpPatch("checklists/{checklistId:guid}")]
        [Authorize(Policy = Policies.CanCheckChecklist)]
        public async Task<IActionResult> UpdateChecklist(
            Guid checklistId, [FromBody] UpdateChecklistRequest req, CancellationToken ct)
        {
            await mediator.Send(
                new UpdatePlacedAssetChecklistCommand(
                    checklistId, req.ChecklistItemId, req.IsChecked, Me()), ct);
            return NoContent();
        }
    }
}
