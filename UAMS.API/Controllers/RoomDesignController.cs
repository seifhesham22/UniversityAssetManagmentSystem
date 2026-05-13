using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Authorization;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using UAMS.Room.Features.AssetDefinitionFeatures.CreateAssetDefinition;
using UAMS.Room.Features.LayoutFeatures;
using UAMS.Room.Features.RoomManagment.CloseRoom;
using UAMS.Room.Features.RoomManagment.CreateRoom;
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
        [HttpPost("assets"), Authorize(Policies.SuperAdminOnly)]
        public async Task<IActionResult> CreateAsset(
            CreateAssetDefinitionCommand command,
            CancellationToken token)
        {
            return Ok(new {Id =  await mediator.Send(command, token) });
        }

        [HttpPost("create-room")]
        [Authorize(Policy = Policies.CanDesignRoom)]
        public async Task<IActionResult> CreateRoom(CreateRoomCommand req)
        {
            return Ok(await mediator.Send(
                new CreateRoomCommand(
                    req.FacultyId,
                    req.BuildingId,
                    Me(), req.name)));
        }

        public sealed record CloseRoomRequest(Guid RoomId, string? Reason);

        [HttpPost("close-room")]
        [Authorize(Policy = Policies.CanDesignRoom)]
        public async Task<IActionResult> CloseRoom(CloseRoomRequest req)
        {
            await mediator.Send(new CloseRoomCommand(Me(), req.RoomId, req.Reason));
            return NoContent();
        }

        [HttpPost("reopen-room/{roomId:guid}")]
        [Authorize(Policy = Policies.CanDesignRoom)]
        public async Task<IActionResult> ReopenRoom(Guid roomId)
        {
            await mediator.Send(new ReopenRoomCommand(Me(), roomId));
            return NoContent();
        }
    }
}