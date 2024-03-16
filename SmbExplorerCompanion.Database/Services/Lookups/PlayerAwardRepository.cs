using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Mappings;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class PlayerAwardRepository : IRepository<PlayerAwardDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public PlayerAwardRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<PlayerAwardDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new PlayerAwardMapping();

        var playerAwards = await _context.PlayerAwards
            .ToListAsync(cancellationToken: cancellationToken);

        var playerAwardDtos = playerAwards
            .Select(mapper.PlayerAwardToPlayerAwardDto)
            .ToList();

        return playerAwardDtos;
    }

    public async Task<PlayerAwardDto> AddAsync(PlayerAwardDto entity, CancellationToken cancellationToken = default)
    {
        var mapper = new PlayerAwardMapping();

        var existingPlayerAward = await _context.PlayerAwards
            .FirstOrDefaultAsync(x => x.Name == entity.Name, cancellationToken: cancellationToken);

        if (existingPlayerAward is not null)
        {
            throw new Exception($"Player award with name '{entity.Name}' already exists.");
        }

        var newPlayerAward = mapper.PlayerAwardDtoToPlayerAward(entity);
        _context.PlayerAwards.Add(newPlayerAward);
        await _context.SaveChangesAsync(cancellationToken);

        var newPlayerAwardDto = mapper.PlayerAwardToPlayerAwardDto(newPlayerAward);
        return newPlayerAwardDto;
    }
}