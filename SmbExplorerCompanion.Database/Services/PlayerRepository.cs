using Microsoft.EntityFrameworkCore;
using OneOf;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services;

public class PlayerRepository : IPlayerRepository
{
    private readonly SmbExplorerCompanionDbContext _dbContext;

    public PlayerRepository(SmbExplorerCompanionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<OneOf<PlayerOverview, Exception>> GetHistoricalPlayer(int playerId, CancellationToken cancellationToken)
    {
        try
        {
            var playerOverviewDto = new PlayerOverview();

            var playerWithSeasons = await _dbContext.Players
                .Include(x => x.Chemistry)
                .Include(x => x.PitcherRole)
                .Include(x => x.ThrowHandedness)
                .Include(x => x.BatHandedness)
                .Include(x => x.PrimaryPosition)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.GameStats)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.BattingStats)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.PitchingStats)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.PitchTypes)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.Traits)
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.PlayerTeamHistory)
                .ThenInclude(x => x.SeasonTeamHistory)
                .ThenInclude(x => x!.Division)
                .ThenInclude(x => x.Conference)
                .Where(x => x.Id == playerId)
                .SingleAsync(cancellationToken: cancellationToken);

            playerOverviewDto.PlayerId = playerWithSeasons.Id;
            playerOverviewDto.PlayerName = $"{playerWithSeasons.FirstName} {playerWithSeasons.LastName}";
            playerOverviewDto.IsPitcher = playerWithSeasons.PitcherRole is not null;
            playerOverviewDto.TotalSalary = playerWithSeasons.PlayerSeasons.Sum(x =>
            {
                var mostRecentTeam = x.PlayerTeamHistory.First(y => y.Order == 1);
                return mostRecentTeam.SeasonTeamHistory is null ? 0 : x.Salary;
            });
            
            // Batting specific stats
            playerOverviewDto.AtBats = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.AtBats));
            playerOverviewDto.Hits = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.Hits));
            playerOverviewDto.HomeRuns = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.HomeRuns));
            playerOverviewDto.BattingAverage = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.AtBats)) == 0
                ? 0
                : playerWithSeasons.PlayerSeasons
                    .Sum(x => x.BattingStats.Sum(y => y.Hits)) /
                  (double) playerWithSeasons.PlayerSeasons
                      .Sum(x => x.BattingStats.Sum(y => y.AtBats));
            playerOverviewDto.Runs = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.Runs));
            playerOverviewDto.RunsBattedIn = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.RunsBattedIn));
            playerOverviewDto.StolenBases = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.StolenBases));
            playerOverviewDto.Obp = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.PlateAppearances)) == 0
                ? 0
                : (playerWithSeasons.PlayerSeasons
                        .Sum(x => x.BattingStats.Sum(y => y.Hits)) +
                    playerWithSeasons.PlayerSeasons
                        .Sum(x => x.BattingStats.Sum(y => y.Walks)) +
                    playerWithSeasons.PlayerSeasons
                        .Sum(x => x.BattingStats.Sum(y => y.HitByPitch))) /
                  (double) (playerWithSeasons.PlayerSeasons
                      .Sum(x => x.BattingStats.Sum(y => y.PlateAppearances)) +
                    playerWithSeasons.PlayerSeasons
                        .Sum(x => x.BattingStats.Sum(y => y.Walks)) +
                    playerWithSeasons.PlayerSeasons
                        .Sum(x => x.BattingStats.Sum(y => y.HitByPitch)) +
                    playerWithSeasons.PlayerSeasons
                        .Sum(x => x.BattingStats.Sum(y => y.SacrificeFlies)));
            playerOverviewDto.Slg = playerWithSeasons.PlayerSeasons
                .Sum(x => x.BattingStats.Sum(y => y.AtBats)) == 0
                ? 0
                : (playerWithSeasons.PlayerSeasons
                        .Sum(x => x.BattingStats.Sum(y => y.Singles)) +
                    (playerWithSeasons.PlayerSeasons
                         .Sum(x => x.BattingStats.Sum(y => y.Doubles)) * 2) +
                    (playerWithSeasons.PlayerSeasons
                         .Sum(x => x.BattingStats.Sum(y => y.Triples)) * 3) +
                    (playerWithSeasons.PlayerSeasons
                         .Sum(x => x.BattingStats.Sum(y => y.HomeRuns)) * 4)) /
                  (double) playerWithSeasons.PlayerSeasons
                      .Sum(x => x.BattingStats.Sum(y => y.AtBats));
            playerOverviewDto.Ops = playerOverviewDto.Obp + playerOverviewDto.Slg;
            playerOverviewDto.OpsPlus = playerWithSeasons.PlayerSeasons
                .SelectMany(x => x.BattingStats)
                .Select(x => new
                {
                    x.OpsPlus,
                    x.GamesBatting,
                    x.GamesPlayed
                })
                .Where(x => x.OpsPlus is not null)
                .Average(x => x.OpsPlus!.Value * (x.GamesBatting / (double) x.GamesPlayed));
            
            // Pitching specific stats
            playerOverviewDto.Wins = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.Wins));
            playerOverviewDto.Losses = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.Losses));
            playerOverviewDto.Era = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.EarnedRuns)) == 0
                ? 0
                : playerWithSeasons.PlayerSeasons
                    .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched ?? 0)) /
                  playerWithSeasons.PlayerSeasons
                      .Sum(x => x.PitchingStats.Sum(y => y.EarnedRuns));
            playerOverviewDto.Games = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.GamesPlayed));
            playerOverviewDto.GamesStarted = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.GamesStarted));
            playerOverviewDto.Saves = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.Saves));
            playerOverviewDto.InningsPitched = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched ?? 0));
            playerOverviewDto.Strikeouts = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.Strikeouts));
            playerOverviewDto.Whip = playerWithSeasons.PlayerSeasons
                .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched)) == 0
                ? 0
                : (playerWithSeasons.PlayerSeasons
                        .Sum(x => x.PitchingStats.Sum(y => y.Hits)) +
                    playerWithSeasons.PlayerSeasons
                        .Sum(x => x.PitchingStats.Sum(y => y.Walks))) /
                  playerWithSeasons.PlayerSeasons
                      .Sum(x => x.PitchingStats.Sum(y => y.InningsPitched ?? 0));
            
            // It would be nice to calculate this in the same way as OPS+ above, but we don't have a number of
            // pitcher appearances stat to use in the calculation, as GamesPlayed is unreliable for pitchers
            playerOverviewDto.EraMinus = playerWithSeasons.PlayerSeasons
                .SelectMany(x => x.PitchingStats)
                .Where(x => x.EraMinus is not null)
                .Average(x => x.EraMinus!.Value);

            throw new NotImplementedException();
        }
        catch (Exception e)
        {
            return e;
        }
    }
}