using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class TraitRepository : IRepository<TraitDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public TraitRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<OneOf<IEnumerable<TraitDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var traits = await _context.Traits
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
        catch (Exception e)
        {
            return e;
        }
    }

    public Task<OneOf<TraitDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<TraitDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<TraitDto, Exception>> AddAsync(TraitDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<IEnumerable<TraitDto>, Exception>> AddRangeAsync(IEnumerable<TraitDto> entities, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<TraitDto, None, Exception>> UpdateAsync(TraitDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<TraitDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}