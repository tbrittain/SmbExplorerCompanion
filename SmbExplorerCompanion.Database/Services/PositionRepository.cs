using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services;

public class PositionRepository : IRepository<PositionDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public PositionRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<OneOf<IEnumerable<PositionDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
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
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<PositionDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        try
        {
            var position = await _context.Positions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);

            if (position == null)
            {
                return new None();
            }

            return new PositionDto
            {
                Id = position.Id,
                Name = position.Name
            };
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public async Task<OneOf<PositionDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var position = await _context.Positions
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.Name == name, cancellationToken);

            if (position == null)
            {
                return new None();
            }

            return new PositionDto
            {
                Id = position.Id,
                Name = position.Name
            };
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public Task<OneOf<PositionDto, Exception>> AddAsync(PositionDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<IEnumerable<PositionDto>, Exception>> AddRangeAsync(IEnumerable<PositionDto> entities,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PositionDto, None, Exception>> UpdateAsync(PositionDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PositionDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}