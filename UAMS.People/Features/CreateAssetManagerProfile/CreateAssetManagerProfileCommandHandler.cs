using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.CreateAssetManagerProfile
{
    public sealed record CreateAssetManagerProfileCommand(
        string fullName,Guid userId ,Guid facultyId) : IRequest<Guid>;

    public sealed class CreateAssetManagerProfileCommandHandler(
        CampusDbContext _db) : IRequestHandler<CreateAssetManagerProfileCommand, Guid>
    {
        public async Task<Guid> Handle(CreateAssetManagerProfileCommand request, CancellationToken cancellationToken)
        {
            if (!await _db.faculties.AnyAsync(x => x.Id == request.facultyId))
                throw new InvalidOperationException(
                    $"Couldn't find a faculty with the Id: {request.facultyId}"
                    );

            if(await _db.asset_managers.AnyAsync(x => x.UserId == request.userId))
                throw new InvalidOperationException(
                    "AssetManagerAlreadyExists"
                    );

            var AssetManager = new AssetManager(
                userId: request.userId,
                facultyId: request.facultyId,
                fullName: request.fullName
                );

            await _db.AddAsync(AssetManager);
            await _db.SaveChangesAsync();
            return AssetManager.Id;
        }
    }
}