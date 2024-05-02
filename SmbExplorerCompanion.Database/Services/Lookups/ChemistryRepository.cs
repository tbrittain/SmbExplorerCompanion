using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class ChemistryRepository : IRepository<ChemistryDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public ChemistryRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<ChemistryDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var chemistry = await _context.Chemistry
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