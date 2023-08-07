using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects.Awards;
using SmbExplorerCompanion.Database.Entities.Lookups;

namespace SmbExplorerCompanion.Database.Services;

public class AwardDelegationRepository : IAwardDelegationRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public AwardDelegationRepository(SmbExplorerCompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    /// <summary>
    /// Automatically assign some awards to players. These are exclusively awards that represent
    /// the leading players in certain categories and the user is not necessary for this assignment
    /// </summary>
    /// <param name="awards">List of system-managed awards to assign</param>
    /// <param name="seasonId"></param>
    /// <param name="cancellationToken"></param>
    private async Task AssignSystemManagedAwards(IReadOnlyCollection<PlayerAward> awards,
        int seasonId,
        CancellationToken cancellationToken = default)
    {
        var season = await _dbContext.Seasons
            .Where(x => x.Id == seasonId)
            .SingleAsync(cancellationToken: cancellationToken);

        var battingIQueryable = _dbContext.PlayerSeasonBattingStats
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Awards)
            .Where(x => x.PlayerSeason.SeasonId == seasonId)
            .Where(x => x.IsRegularSeason);

        // clear the Awards collection for each player season where there are existing awards
        // this is necessary because we will be re-assigning awards
        // and this may occur multiple times if the user is re-running the award assignment
        foreach (var playerSeason in await battingIQueryable
                     .Where(x => x.PlayerSeason.Awards.Any())
                     .Select(x => x.PlayerSeason)
                     .ToListAsync(cancellationToken: cancellationToken))
        {
            playerSeason.Awards.Clear();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var maxHomeRuns = await battingIQueryable
            .MaxAsync(x => x.HomeRuns, cancellationToken: cancellationToken);
        var topHomeRunHitters = await battingIQueryable
            .Where(x => x.HomeRuns == maxHomeRuns)
            .ToListAsync(cancellationToken: cancellationToken);

        var maxRbi = await battingIQueryable
            .MaxAsync(x => x.RunsBattedIn, cancellationToken: cancellationToken);
        var topRbiHitters = await battingIQueryable
            .Where(x => x.RunsBattedIn == maxRbi)
            .ToListAsync(cancellationToken: cancellationToken);

        var maxQualifyingBattingAverage = await battingIQueryable
            .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
            .Where(x => x.BattingAverage != null)
            .MaxAsync(x => x.BattingAverage, cancellationToken: cancellationToken);

        const double delta = 0.01;
        var topBattingAverageHitters = await battingIQueryable
            .Where(x => x.BattingAverage != null && Math.Abs(x.BattingAverage.Value - maxQualifyingBattingAverage!.Value) < delta)
            .Where(x => x.PlateAppearances >= season.NumGamesRegularSeason * 3.1)
            .OrderByDescending(x => x.BattingAverage)
            .ToListAsync(cancellationToken: cancellationToken);

        // here, we need to check if a single player is the leader for all 3 categories, for which they will
        // be the triple crown winner. if they share an individual category with another player, we will still give
        // the original player the triple crown, but the other player will also get the award for that category
        var battingTripleCrownWinner = topBattingAverageHitters
            .Select(x => x.PlayerSeasonId)
            .Intersect(topHomeRunHitters
                .Select(x => x.PlayerSeasonId))
            .Intersect(topRbiHitters
                .Select(x => x.PlayerSeasonId))
            .FirstOrDefault();

        // highly unlikely that there will be a tie for the triple crown, so we just take the first player
        // the batting average should be a decent tiebreaker since its difference is much more nuanced
        if (battingTripleCrownWinner != default)
        {
            var battingTripleCrownAward = awards.First(x => x.OriginalName == "Triple Crown (Batting)");
            var battingTripleCrownPlayerSeason = await _dbContext.PlayerSeasons
                .Include(x => x.Awards)
                .SingleAsync(x => x.Id == battingTripleCrownWinner, cancellationToken: cancellationToken);

            battingTripleCrownPlayerSeason.Awards.Add(battingTripleCrownAward);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        // now we can assign the awards to the players who are not the triple crown winner, if any
        foreach (var batterStats in topHomeRunHitters
                     .Where(x => x.PlayerSeasonId != battingTripleCrownWinner))
        {
            var award = awards.First(x => x.OriginalName == "Home Run Title");
            var playerSeason = await _dbContext.PlayerSeasons
                .Include(x => x.Awards)
                .SingleAsync(x => x.Id == batterStats.PlayerSeasonId, cancellationToken: cancellationToken);

            playerSeason.Awards.Add(award);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var batterStats in topRbiHitters
                     .Where(x => x.PlayerSeasonId != battingTripleCrownWinner))
        {
            var award = awards.First(x => x.OriginalName == "RBI Title");
            var playerSeason = await _dbContext.PlayerSeasons
                .Include(x => x.Awards)
                .SingleAsync(x => x.Id == batterStats.PlayerSeasonId, cancellationToken: cancellationToken);

            playerSeason.Awards.Add(award);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var batterStats in topBattingAverageHitters
                     .Where(x => x.PlayerSeasonId != battingTripleCrownWinner))
        {
            var award = awards.First(x => x.OriginalName == "Batting Title");
            var playerSeason = await _dbContext.PlayerSeasons
                .Include(x => x.Awards)
                .SingleAsync(x => x.Id == batterStats.PlayerSeasonId, cancellationToken: cancellationToken);

            playerSeason.Awards.Add(award);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        var pitchingIQueryable = _dbContext.PlayerSeasonPitchingStats
            .Include(x => x.PlayerSeason)
            .ThenInclude(x => x.Awards)
            .Where(x => x.PlayerSeason.SeasonId == seasonId)
            .Where(x => x.IsRegularSeason);

        foreach (var playerSeason in await pitchingIQueryable
                     .Where(x => x.PlayerSeason.Awards.Any())
                     .Select(x => x.PlayerSeason)
                     .ToListAsync(cancellationToken: cancellationToken))
        {
            playerSeason.Awards.Clear();
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        var minEarnedRunAverage = await pitchingIQueryable
            .Where(x => x.InningsPitched >= season.NumGamesRegularSeason * 1.0)
            .Where(x => x.EarnedRunAverage != null)
            .MinAsync(x => x.EarnedRunAverage, cancellationToken: cancellationToken);

        var topEarnedRunAveragePitchers = await pitchingIQueryable
            .Where(x => x.EarnedRunAverage != null && Math.Abs(x.EarnedRunAverage.Value - minEarnedRunAverage!.Value) < delta)
            .Where(x => x.InningsPitched >= season.NumGamesRegularSeason * 1.0)
            .OrderBy(x => x.EarnedRunAverage)
            .ToListAsync(cancellationToken: cancellationToken);

        var maxWins = await pitchingIQueryable
            .MaxAsync(x => x.Wins, cancellationToken: cancellationToken);

        var topWinsPitchers = await pitchingIQueryable
            .Where(x => x.Wins == maxWins)
            .OrderByDescending(x => x.Wins)
            .ToListAsync(cancellationToken: cancellationToken);

        var maxStrikeouts = await pitchingIQueryable
            .MaxAsync(x => x.Strikeouts, cancellationToken: cancellationToken);

        var topStrikeoutsPitchers = await pitchingIQueryable
            .Where(x => x.Strikeouts == maxStrikeouts)
            .OrderByDescending(x => x.Strikeouts)
            .ToListAsync(cancellationToken: cancellationToken);

        var pitchingTripleCrownWinner = topEarnedRunAveragePitchers
            .Select(x => x.PlayerSeasonId)
            .Intersect(topWinsPitchers
                .Select(x => x.PlayerSeasonId))
            .Intersect(topStrikeoutsPitchers
                .Select(x => x.PlayerSeasonId))
            .FirstOrDefault();

        if (pitchingTripleCrownWinner != default)
        {
            var pitchingTripleCrownAward = awards.First(x => x.OriginalName == "Triple Crown (Pitching)");
            var pitchingTripleCrownPlayerSeason = await _dbContext.PlayerSeasons
                .Include(x => x.Awards)
                .SingleAsync(x => x.Id == pitchingTripleCrownWinner, cancellationToken: cancellationToken);

            pitchingTripleCrownPlayerSeason.Awards.Add(pitchingTripleCrownAward);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var pitcherStats in topEarnedRunAveragePitchers
                     .Where(x => x.PlayerSeasonId != pitchingTripleCrownWinner))
        {
            var award = awards.First(x => x.OriginalName == "ERA Title");
            var playerSeason = await _dbContext.PlayerSeasons
                .Include(x => x.Awards)
                .SingleAsync(x => x.Id == pitcherStats.PlayerSeasonId, cancellationToken: cancellationToken);

            playerSeason.Awards.Add(award);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var pitcherStats in topWinsPitchers
                     .Where(x => x.PlayerSeasonId != pitchingTripleCrownWinner))
        {
            var award = awards.First(x => x.OriginalName == "Wins Title");
            var playerSeason = await _dbContext.PlayerSeasons
                .Include(x => x.Awards)
                .SingleAsync(x => x.Id == pitcherStats.PlayerSeasonId, cancellationToken: cancellationToken);

            playerSeason.Awards.Add(award);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        foreach (var pitcherStats in topStrikeoutsPitchers
                     .Where(x => x.PlayerSeasonId != pitchingTripleCrownWinner))
        {
            var award = awards.First(x => x.OriginalName == "Strikeouts Title");
            var playerSeason = await _dbContext.PlayerSeasons
                .Include(x => x.Awards)
                .SingleAsync(x => x.Id == pitcherStats.PlayerSeasonId, cancellationToken: cancellationToken);

            playerSeason.Awards.Add(award);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task<OneOf<Success, Exception>> AddRegularSeasonPlayerAwards(int seasonId,
        List<PlayerAwardRequestDto> playerAwardRequestDtos,
        CancellationToken cancellationToken = default)
    {
        var awardsByPlayerId = playerAwardRequestDtos
            .GroupBy(x => x.PlayerId)
            .ToList();

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var regularSeasonAwards = await _dbContext.PlayerAwards
                .Where(x => !x.IsPlayoffAward)
                .ToListAsync(cancellationToken: cancellationToken);

            var allAwardIds = regularSeasonAwards
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

            var automaticallyAssignedAwards = regularSeasonAwards
                .Where(x => !x.IsUserAssignable)
                .ToList();

            invalidAwardIds = playerAwardRequestDtos
                .Select(x => x.AwardId)
                .Intersect(automaticallyAssignedAwards.Select(x => x.Id))
                .ToList();

            if (invalidAwardIds.Any())
            {
                await transaction.RollbackAsync(cancellationToken);
                return new Exception($"Invalid award IDs, must be automatically " +
                                     $"assigned by the system: {string.Join(", ", invalidAwardIds)}");
            }

            await AssignSystemManagedAwards(automaticallyAssignedAwards, seasonId, cancellationToken);

            foreach (var awardsByPlayer in awardsByPlayerId)
            {
                var playerId = awardsByPlayer.Key;
                var newPlayerAwardIds = awardsByPlayer
                    .Select(x => x.AwardId)
                    .ToList();

                var newPlayerAwards = regularSeasonAwards
                    .Where(x => newPlayerAwardIds.Contains(x.Id))
                    .ToList();

                var playerSeason = await _dbContext.PlayerSeasons
                    .Include(x => x.Awards)
                    .FirstAsync(x => x.PlayerId == playerId &&
                                     x.SeasonId == seasonId,
                        cancellationToken: cancellationToken);

                foreach (var award in newPlayerAwards) playerSeason.Awards.Add(award);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            return new Success();
        }
        catch (Exception e)
        {
            await transaction.RollbackAsync(cancellationToken);
            return e;
        }
    }
}