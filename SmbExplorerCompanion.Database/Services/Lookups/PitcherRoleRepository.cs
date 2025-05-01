using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class PitcherRoleRepository(SmbExplorerCompanionDbContext context) : IGetAllRepository<PitcherRoleDto>
{
    public async Task<IEnumerable<PitcherRoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var pitcherRoles = await context.PitcherRoles
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return pitcherRoles
            .Select(p => new PitcherRoleDto
            {
                Id = p.Id,
                Name = p.Name
            })
            .ToList();
    }
}