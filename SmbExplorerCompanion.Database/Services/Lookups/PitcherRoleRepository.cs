using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
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

    public async Task<OneOf<IEnumerable<PitcherRoleDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<PitcherRoleDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var pitcherRole = await _context.PitcherRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (pitcherRole is null)
            {
                return new None();
            }

            return new PitcherRoleDto
            {
                Id = pitcherRole.Id,
                Name = pitcherRole.Name
            };
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<PitcherRoleDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var pitcherRole = await _context.PitcherRoles
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

            if (pitcherRole is null)
            {
                return new None();
            }

            return new PitcherRoleDto
            {
                Id = pitcherRole.Id,
                Name = pitcherRole.Name
            };
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public Task<OneOf<PitcherRoleDto, Exception>> AddAsync(PitcherRoleDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<IEnumerable<PitcherRoleDto>, Exception>> AddRangeAsync(IEnumerable<PitcherRoleDto> entities, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PitcherRoleDto, None, Exception>> UpdateAsync(PitcherRoleDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PitcherRoleDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}