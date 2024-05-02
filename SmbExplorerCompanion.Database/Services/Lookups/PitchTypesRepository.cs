using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class PitchTypesRepository : IRepository<PitchTypeDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public PitchTypesRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PitchTypeDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var pitchTypes = await _context.PitchTypes
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

    public Task<PitchTypeDto> AddAsync(PitchTypeDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}