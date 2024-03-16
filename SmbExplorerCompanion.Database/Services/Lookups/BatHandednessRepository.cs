using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class BatHandednessRepository : IRepository<BatHandednessDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public BatHandednessRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<BatHandednessDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var batHandedness = await _context.BatHandedness
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

    public Task<BatHandednessDto> AddAsync(BatHandednessDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}