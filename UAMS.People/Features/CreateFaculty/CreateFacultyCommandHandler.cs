using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using UAMS.Campus.Models;
using UAMS.Campus.Presistence;

namespace UAMS.Campus.Features.CreateFaculty
{
    public sealed record CreateFacultyCommand(string name) : IRequest<Guid>;
    public sealed class CreateFacultyCommandHandler(CampusDbContext _db)
        : IRequestHandler<CreateFacultyCommand, Guid>
    {
        public async Task<Guid> Handle(CreateFacultyCommand request, CancellationToken cancellationToken)
        {
            var duplicate = await _db.faculties
                .AnyAsync(x => x.Name == request.name);

            if (duplicate)
                throw new InvalidOperationException($"faculty: {request.name} already exists");

            var newFaculty = new Faculty(request.name);
            await _db.faculties.AddAsync(newFaculty);
            await _db.SaveChangesAsync(cancellationToken);

            return newFaculty.Id;
        }
    }
}