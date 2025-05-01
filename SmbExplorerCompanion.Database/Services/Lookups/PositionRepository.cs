using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class PositionRepository(SmbExplorerCompanionDbContext context) : IGetAllRepository<PositionDto>
{
    public async Task<IEnumerable<PositionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var positions = await context.Positions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return positions
            .Select(p => new PositionDto
            {
                Id = p.Id,
                Name = p.Name,
                IsPrimaryPosition = p.IsPrimaryPosition
            })
            .ToList();
    }
}