using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
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

    public async Task<OneOf<IEnumerable<PitchTypeDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception e)
        {
            return e;
        }
    }

    public Task<OneOf<PitchTypeDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PitchTypeDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PitchTypeDto, Exception>> AddAsync(PitchTypeDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<IEnumerable<PitchTypeDto>, Exception>> AddRangeAsync(IEnumerable<PitchTypeDto> entities,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PitchTypeDto, None, Exception>> UpdateAsync(PitchTypeDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PitchTypeDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}