using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using UAMS.Campus.Features.ListBuildingsQuery;
using UAMS.Campus.Features.ListFacultiesQuery;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.CanViewFaculties)]
    public class CampusController(IMediator mediator) : ControllerBase
    {
        [HttpGet("buildings")]
        public async Task<IActionResult> ListBuildings(
            [FromQuery] string? search,
            CancellationToken ct = default,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var res = await mediator.Send(new ListBuildingQuery(search, page, pageSize), ct);
            return Ok(res);
        }

        [HttpGet("faculties")]
        public async Task<IActionResult> ListFaculties(
            [FromQuery] string? search,
            CancellationToken ct = default,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var res = await mediator.Send(new ListFacultiesQuery(search, page, pageSize), ct);
            return Ok(res);
        }
    }
}