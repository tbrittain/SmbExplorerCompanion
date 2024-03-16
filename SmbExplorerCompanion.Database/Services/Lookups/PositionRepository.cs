using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class PositionRepository : IRepository<PositionDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public PositionRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PositionDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var positions = await _context.Positions
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return positions
            .Select(p => new PositionDto
            {
                Id = p.Id,
                Name = p.Name,
                IsPrimaryPosition = p.IsPrimaryPosition
            })
            .ToList();
    }

    public Task<PositionDto> AddAsync(PositionDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}