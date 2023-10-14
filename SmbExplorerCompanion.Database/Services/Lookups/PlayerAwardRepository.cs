using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Lookups;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Database.Entities.Lookups;
using SmbExplorerCompanion.Database.Mappings;

namespace SmbExplorerCompanion.Database.Services.Lookups;

public class PlayerAwardRepository : IRepository<PlayerAwardDto>
{
    private readonly SmbExplorerCompanionDbContext _context;

    public PlayerAwardRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<OneOf<IEnumerable<PlayerAwardDto>, Exception>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var mapper = new PlayerAwardMapping();

        List<PlayerAward> playerAwards;
        try
        {
            playerAwards = await _context.PlayerAwards
                .ToListAsync(cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            return e;
        }

        var playerAwardDtos = playerAwards
            .Select(mapper.PlayerAwardToPlayerAwardDto)
            .ToList();

        return playerAwardDtos;
    }

    public async Task<OneOf<PlayerAwardDto, None, Exception>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var mapper = new PlayerAwardMapping();

        PlayerAward? playerAward;
        try
        {
            playerAward = await _context.PlayerAwards
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            return e;
        }

        if (playerAward is null)
        {
            return new None();
        }

        var playerAwardDto = mapper.PlayerAwardToPlayerAwardDto(playerAward);
        return playerAwardDto;
    }

    public async Task<OneOf<PlayerAwardDto, None, Exception>> GetByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var mapper = new PlayerAwardMapping();

        PlayerAward? playerAward;
        try
        {
            playerAward = await _context.PlayerAwards
                .FirstOrDefaultAsync(x => x.Name == name, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            return e;
        }

        if (playerAward is null)
        {
            return new None();
        }

        var playerAwardDto = mapper.PlayerAwardToPlayerAwardDto(playerAward);
        return playerAwardDto;
    }

    public async Task<OneOf<PlayerAwardDto, Exception>> AddAsync(PlayerAwardDto entity, CancellationToken cancellationToken = default)
    {
        var mapper = new PlayerAwardMapping();

        PlayerAward? playerAward;
        try
        {
            playerAward = await _context.PlayerAwards
                .FirstOrDefaultAsync(x => x.Name == entity.Name, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            return e;
        }

        if (playerAward is not null)
        {
            return new Exception($"Player award with name '{entity.Name}' already exists.");
        }

        var newPlayerAward = mapper.PlayerAwardDtoToPlayerAward(entity);
        try
        {
            await _context.PlayerAwards.AddAsync(newPlayerAward, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return e;
        }

        var newPlayerAwardDto = mapper.PlayerAwardToPlayerAwardDto(newPlayerAward);
        return newPlayerAwardDto;
    }

    public Task<OneOf<IEnumerable<PlayerAwardDto>, Exception>> AddRangeAsync(IEnumerable<PlayerAwardDto> entities,
        CancellationToken cancellationToken = default)
    {
        throw new NotSupportedException();
    }

    public async Task<OneOf<PlayerAwardDto, None, Exception>> UpdateAsync(PlayerAwardDto entity, CancellationToken cancellationToken = default)
    {
        var mapper = new PlayerAwardMapping();

        PlayerAward? playerAward;
        try
        {
            playerAward = await _context.PlayerAwards
                .FirstOrDefaultAsync(x => x.Id == entity.Id, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            return e;
        }

        if (playerAward is null)
        {
            return new None();
        }

        var updatedPlayerAward = mapper.PlayerAwardDtoToPlayerAward(entity);
        try
        {
            _context.PlayerAwards.Update(updatedPlayerAward);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return e;
        }

        var updatedPlayerAwardDto = mapper.PlayerAwardToPlayerAwardDto(updatedPlayerAward);
        return updatedPlayerAwardDto;
    }

    public async Task<OneOf<PlayerAwardDto, None, Exception>> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var mapper = new PlayerAwardMapping();
        
        PlayerAward? playerAward;
        try
        {
            playerAward = await _context.PlayerAwards
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken: cancellationToken);
        }
        catch (Exception e)
        {
            return e;
        }
        
        if (playerAward is null)
        {
            return new None();
        }
        
        try
        {
            _context.PlayerAwards.Remove(playerAward);
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            return e;
        }
        
        var playerAwardDto = mapper.PlayerAwardToPlayerAwardDto(playerAward);
        return playerAwardDto;
    }
}