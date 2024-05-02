using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class PitcherRoleRepository : IRepository<PitcherRoleDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public PitcherRoleRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PitcherRoleDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var pitcherRoles = await _context.PitcherRoles
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

    public Task<PitcherRoleDto> AddAsync(PitcherRoleDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}