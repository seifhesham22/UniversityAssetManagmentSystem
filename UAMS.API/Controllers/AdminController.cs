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
using UAMS.Campus.Features.AssignTeacherToFaculty;
using UAMS.Campus.Features.CreateAssetManagerProfile;
using UAMS.Campus.Features.CreateBuilding;
using UAMS.Campus.Features.CreateDeapartment;
using UAMS.Campus.Features.CreateDepartmentManagerProfile;
using UAMS.Campus.Features.CreateFaculty;
using UAMS.Campus.Features.CreateStudentProfile;
using UAMS.Campus.Features.LinkFacultyToBuilding;
using UAMS.Campus.Features.ListBuildingsQuery;
using UAMS.Campus.Features.ListDepartments;
using UAMS.Campus.Features.ListFacultiesQuery;
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
        [HttpPost("faculties")]
        public async Task<IActionResult> CreateFaculty([Required][NotNull][MaxLength(80)]string name)
        {
            var res = await mediator.Send(new CreateFacultyCommand(name));
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

        [HttpGet("departments")]
        public async Task<IActionResult> ListDepartments(
            [FromQuery] string? search,
            [FromQuery] AssetCategory Category,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken ct = default)
        {
            var res = await mediator.Send(new ListDepartmentCommand(search,Category,page,pageSize), ct);
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
    }
}