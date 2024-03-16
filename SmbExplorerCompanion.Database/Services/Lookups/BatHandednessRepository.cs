using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
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

    public async Task<OneOf<IEnumerable<BatHandednessDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception e)
        {
            return e;
        }
    }

    public Task<OneOf<BatHandednessDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<BatHandednessDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<BatHandednessDto, Exception>> AddAsync(BatHandednessDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<IEnumerable<BatHandednessDto>, Exception>> AddRangeAsync(IEnumerable<BatHandednessDto> entities, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<BatHandednessDto, None, Exception>> UpdateAsync(BatHandednessDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<BatHandednessDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}