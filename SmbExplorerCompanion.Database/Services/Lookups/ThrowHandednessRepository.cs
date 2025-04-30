using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class ThrowHandednessRepository(SmbExplorerCompanionDbContext context) : IRepository<ThrowHandednessDto>
{
    public async Task<IEnumerable<ThrowHandednessDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var throwHandedness = await context.ThrowHandedness
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return throwHandedness
            .Select(p => new ThrowHandednessDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToList();
    }

    public Task<ThrowHandednessDto> AddAsync(ThrowHandednessDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}