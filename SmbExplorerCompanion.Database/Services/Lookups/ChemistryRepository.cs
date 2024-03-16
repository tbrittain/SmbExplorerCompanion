using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class ChemistryRepository : IRepository<ChemistryDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public ChemistryRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<OneOf<IEnumerable<ChemistryDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var chemistry = await _context.Chemistry
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return chemistry
                .Select(p => new ChemistryDto
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

    public Task<OneOf<ChemistryDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<ChemistryDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<ChemistryDto, Exception>> AddAsync(ChemistryDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<IEnumerable<ChemistryDto>, Exception>> AddRangeAsync(IEnumerable<ChemistryDto> entities, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<ChemistryDto, None, Exception>> UpdateAsync(ChemistryDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<ChemistryDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}