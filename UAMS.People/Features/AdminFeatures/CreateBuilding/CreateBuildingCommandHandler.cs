using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.AdminFeatures.CreateBuilding
{
    public sealed record CreateBuildingCommand(string name, string address) : IRequest<Guid>;
    public sealed class CreateBuildingCommandHandler(CampusDbContext _db)
        : IRequestHandler<CreateBuildingCommand, Guid>
    {
        public async Task<Guid> Handle(CreateBuildingCommand request, CancellationToken cancellationToken)
        {
            var duplicate = await _db.buildings
                .AnyAsync(b => b.Name == request.name);

            if (duplicate)
                throw new InvalidOperationException($"building: {request.name} already exists");

            var newBuilding = new Building(
                name: request.name,
                address: request.address);

            await _db.buildings.AddAsync(newBuilding);
            await _db.SaveChangesAsync(cancellationToken);

            return newBuilding.Id;
        }
    }
}