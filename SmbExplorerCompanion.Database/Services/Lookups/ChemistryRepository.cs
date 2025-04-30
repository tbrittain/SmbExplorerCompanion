using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class ChemistryRepository(SmbExplorerCompanionDbContext context) : IRepository<ChemistryDto>
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

    public Task<ChemistryDto> AddAsync(ChemistryDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}