using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class ChemistryRepository(SmbExplorerCompanionDbContext context) : IGetAllRepository<ChemistryDto>
{
    public async Task<IEnumerable<ChemistryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var chemistry = await context.Chemistry
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return chemistry
            .Select(p => new ChemistryDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToList();
    }
}