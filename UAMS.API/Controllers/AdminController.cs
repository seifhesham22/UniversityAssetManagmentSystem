using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Shared.Abstractions.Policy;
using Shared.Enums;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Security.Cryptography.X509Certificates;
using System.Security.Principal;
using UAMS.API.DTOs.Admin;
using UAMS.Campus.Features.AdminFeatures.CreateAssetManagerProfile;
using UAMS.Campus.Features.AdminFeatures.CreateBuilding;
using UAMS.Campus.Features.AdminFeatures.CreateDeapartment;
using UAMS.Campus.Features.AdminFeatures.CreateDepartmentManagerProfile;
using UAMS.Campus.Features.AdminFeatures.CreateFaculty;
using UAMS.Campus.Features.AdminFeatures.LinkFacultyToBuilding;
using UAMS.Campus.Features.AdminFeatures.GetAdminStats;
using UAMS.Campus.Features.AdminFeatures.ListAdminFaculties;
using UAMS.Campus.Features.AdminFeatures.ListAssetManagers;
using UAMS.Campus.Features.AdminFeatures.ListBuildingsQuery;
using UAMS.Campus.Features.AdminFeatures.ListDepartmentManagers;
using UAMS.Campus.Features.AdminFeatures.ReassignAssetManager;
using UAMS.Campus.Features.AdminFeatures.RemoveAssetManager;
using UAMS.Campus.Features.AdminFeatures.UnLinkFacultyFromBuilding;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;
using UAMS.Identity.Services.AuthService;

namespace UAMS.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Policy = Policies.SuperAdminOnly)]
    public class AdminController(IMediator mediator, IAuthService _auth) : ControllerBase
    {
        public sealed record CreateFacultyRequest(string Name);

        [HttpGet("faculties")]
        public async Task<IActionResult> ListAdminFaculties(
            [FromQuery] string? search,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await mediator.Send(new ListAdminFacultiesQuery(search, page, pageSize), ct);
            return Ok(res);
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats(CancellationToken ct = default)
        {
            var res = await mediator.Send(new GetAdminStatsQuery(), ct);
            return Ok(res);
        }

        [HttpPost("faculties")]
        public async Task<IActionResult> CreateFaculty([FromBody] CreateFacultyRequest req)
        {
            var res = await mediator.Send(new CreateFacultyCommand(req.Name));
            return Ok(res);
        }

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

        [HttpPost("buildings")]
        public async Task<IActionResult> CreateBuilding(CreateBuidlingRequest req, CancellationToken ct = default)
        {
            var res = await mediator.Send(new CreateBuildingCommand(req.name, req.address),ct);
            return Ok(res);
        }


        [HttpPost("link-faculty")]
        public async Task<IActionResult> LinkFaculties(LinkFacultyToBuildingRequest req, CancellationToken ct = default)
        {
            await mediator.Send(new LinkFacultyToBuildingCommand(req.buildingId, req.facultyId), ct);
            return NoContent();
        }

        [HttpPost("departments")]
        public async Task<IActionResult> CreateDepartment(
        CreateDepartmentCommand cmd, CancellationToken ct = default)
        {
            var res = await mediator.Send(cmd, ct);
            return Ok(res);
        }

        [HttpPost("asset-managers")]
        public async Task<IActionResult> CreateAssetManager(
        CreateAssetManagerRequest req, CancellationToken ct)
        {
            var userId = await _auth.CreateUserAsync(req.email, req.password, Role.AssetManager, ct);
            try
            {
                var profileId = await mediator.Send(
                    new CreateAssetManagerProfileCommand(req.fullName,userId ,req.facultyId), ct);
                return Ok(new { UserId = userId, ProfileId = profileId });
            }
            catch
            {
                await _auth.DeleteUserAsync(userId, ct);
                throw;
            }
        }


        [HttpPost("department-managers")]
        public async Task<IActionResult> CreateDepartmentManager(
        CreateDeptManagerRequest req, CancellationToken ct)
        {
            var userId = await _auth.CreateUserAsync(
                req.email, req.password, Role.DepartmentManager, ct);
            try
            {
                var profileId = await mediator.Send(
                    new CreateDepartmentManagerCommand(userId, req.departmentId, req.fullName), ct);
                return Ok(new { UserId = userId, ProfileId = profileId });
            }
            catch
            {
                await _auth.DeleteUserAsync(userId, ct);
                throw;
            }
        }

        [HttpDelete("unlink-faculty")]
        public async Task<IActionResult> UnlinkFaculty(
            UnLinkFacultyToBuildingRequest request,
            CancellationToken ct)
        {
            await mediator.Send(new UnlinkFacultyCommand(request.facultyId, request.buildingId));
            return NoContent();
        }


        [HttpGet("asset-managers")]
        public async Task<IActionResult> ListAssetManagers(
            [FromQuery] string? search,
            [FromQuery] Guid? facultyId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await mediator.Send(
                new ListAssetManagersQuery(search, facultyId, page, pageSize), ct);
            return Ok(res);
        }

        [HttpPut("asset-managers/{id:guid}/faculty")]
        public async Task<IActionResult> ReassignAssetManager(
            Guid id,
            [FromBody] ReassignRequest req,
            CancellationToken ct = default)
        {
            await mediator.Send(new ReassignAssetManagerCommand(id, req.FacultyId), ct);
            return NoContent();
        }

        [HttpDelete("asset-managers/{id:guid}")]
        public async Task<IActionResult> RemoveAssetManager(Guid id, CancellationToken ct = default)
        {
            await mediator.Send(new RemoveAssetManagerCommand(id), ct);
            return NoContent();
        }


        [HttpGet("dept-managers")]
        public async Task<IActionResult> ListDeptManagers(
            [FromQuery] string? search,
            [FromQuery] Guid? departmentId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await mediator.Send(
                new ListDepartmentManagersQuery(search, departmentId, page, pageSize), ct);
            return Ok(res);
        }

        public sealed record ReassignRequest(Guid FacultyId);
    }
}