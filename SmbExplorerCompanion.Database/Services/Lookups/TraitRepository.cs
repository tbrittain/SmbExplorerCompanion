using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class TraitRepository(SmbExplorerCompanionDbContext context) : IRepository<TraitDto>
{
    public async Task<IEnumerable<TraitDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var traits = await context.Traits
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return traits
            .Select(p => new TraitDto
            {
                Id = p.Id,
                Name = p.Name,
                IsSmb3 = p.IsSmb3,
                IsPositive = p.IsPositive
            })
            .ToList();
    }

    public Task<TraitDto> AddAsync(TraitDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}