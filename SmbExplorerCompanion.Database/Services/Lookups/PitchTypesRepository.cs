using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class PitchTypesRepository(SmbExplorerCompanionDbContext context) : IGetAllRepository<PitchTypeDto>
{
    public async Task<IEnumerable<PitchTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var pitchTypes = await context.PitchTypes
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return pitchTypes
            .Select(p => new PitchTypeDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToList();
    }
}