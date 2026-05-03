using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using UAMS.Campus.Features.ListFacultiesQuery;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudentsController(IMediator mediator) : ControllerBase
    {
        [HttpGet("faculties")]
        [Authorize(Policy = Policies.CanViewFaculties)]
        public async Task<IActionResult> GetFaculties(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var res = await mediator.Send(new ListFacultiesQuery(search, page, pageSize));
            return Ok(res);
        }
    }
}