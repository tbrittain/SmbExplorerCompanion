using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class AwardRepository : IRepository<PlayerAwardDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public AwardRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<OneOf<IEnumerable<PlayerAwardDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var awards = await _context.PlayerAwards
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return awards
                .Select(p => new PlayerAwardDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Importance = p.Importance,
                    OmitFromGroupings = p.OmitFromGroupings,
                    OriginalName = p.OriginalName,
                    IsBuiltIn = p.IsBuiltIn,
                    IsBattingAward = p.IsBattingAward,
                    IsPitchingAward = p.IsPitchingAward,
                    IsFieldingAward = p.IsFieldingAward,
                    IsPlayoffAward = p.IsPlayoffAward,
                    IsUserAssignable = p.IsUserAssignable
                })
                .ToList();
        }
        catch (Exception e)
        {
            return e;
        }
    }

    public Task<OneOf<PlayerAwardDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PlayerAwardDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PlayerAwardDto, Exception>> AddAsync(PlayerAwardDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<IEnumerable<PlayerAwardDto>, Exception>> AddRangeAsync(IEnumerable<PlayerAwardDto> entities, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PlayerAwardDto, None, Exception>> UpdateAsync(PlayerAwardDto entity, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public Task<OneOf<PlayerAwardDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }
}