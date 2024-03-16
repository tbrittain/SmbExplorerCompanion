using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class ThrowHandednessRepository : IRepository<ThrowHandednessDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public ThrowHandednessRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<OneOf<IEnumerable<ThrowHandednessDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var throwHandedness = await _context.ThrowHandedness
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
        catch (Exception e)
        {
            return e;
        }
    }

    public Task<OneOf<ThrowHandednessDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<ThrowHandednessDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<ThrowHandednessDto, Exception>> AddAsync(ThrowHandednessDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<IEnumerable<ThrowHandednessDto>, Exception>> AddRangeAsync(IEnumerable<ThrowHandednessDto> entities,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<ThrowHandednessDto, None, Exception>> UpdateAsync(ThrowHandednessDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<ThrowHandednessDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}