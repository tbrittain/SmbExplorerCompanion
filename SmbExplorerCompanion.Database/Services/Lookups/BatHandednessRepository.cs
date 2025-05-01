using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class BatHandednessRepository(SmbExplorerCompanionDbContext context) : IGetAllRepository<BatHandednessDto>
{
    public async Task<IEnumerable<BatHandednessDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var batHandedness = await context.BatHandedness
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return batHandedness
            .Select(p => new BatHandednessDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToList();
    }
}