using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Shared.Enums;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class AwardRepository : IRepository<PlayerAwardDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public AwardRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PlayerAwardDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var awards = await _context.PlayerAwards
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var dtos = awards
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

        // add virtual awards, like Hall of Famer and Champion, which are not technically awards
        // but treated like so in many places
        dtos.Add(new PlayerAwardDto
        {
            Id = (int)VirtualAward.Champion,
            Name = "Champion",
            OriginalName = "Champion",
            Importance = 10,
            OmitFromGroupings = false,
        });

        dtos.Add(new PlayerAwardDto
        {
            Id = (int)VirtualAward.HallOfFame,
            Importance = -1,
            Name = "Hall of Fame",
            OriginalName = "Hall of Fame",
            OmitFromGroupings = false
        });

        return dtos;
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

    public Task<OneOf<IEnumerable<PlayerAwardDto>, Exception>> AddRangeAsync(IEnumerable<PlayerAwardDto> entities,
        CancellationToken cancellationToken = default)
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