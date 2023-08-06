using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;

namespace SmbExplorerCompanion.Database.Services;

public class AwardRepository : IAwardRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public AwardRepository(SmbExplorerCompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<Success, Exception>> AddPlayerAwards(int seasonId,
        List<PlayerAwardRequestDto> playerAwardRequestDtos,CancellationToken cancellationToken = default)
    {
        var awardsByPlayerId = playerAwardRequestDtos
            .GroupBy(x => x.PlayerId)
            .ToList();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var awards = await _dbContext.PlayerAwards
                .ToListAsync(cancellationToken: cancellationToken);

            var allAwardIds = awards
                .Select(x => x.Id)
                .ToList();
            
            var invalidAwardIds = playerAwardRequestDtos
                .Select(x => x.AwardId)
                .Except(allAwardIds)
                .ToList();
            
            if (invalidAwardIds.Any())
            {
                await transaction.RollbackAsync(cancellationToken);
                return new Exception($"Invalid award IDs: {string.Join(", ", invalidAwardIds)}");
            }

            // initialize a database transaction
            foreach (var awardsByPlayer in awardsByPlayerId)
            {
                var playerId = awardsByPlayer.Key;
                var newPlayerAwardIds= awardsByPlayer
                    .Select(x => x.AwardId)
                    .ToList();

                var newPlayerAwards = awards
                    .Where(x => newPlayerAwardIds.Contains(x.Id))
                    .ToList();
                
                var playerSeason = await _dbContext.PlayerSeasons
                    .Include(x => x.Awards)
                    .FirstAsync(x => x.PlayerId == playerId && 
                                              x.SeasonId == seasonId, cancellationToken: cancellationToken);

                playerSeason.Awards.Clear();
                await _dbContext.SaveChangesAsync(cancellationToken);
                foreach (var award in newPlayerAwards) playerSeason.Awards.Add(award);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return new Success();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            return e;
        }
    }
}