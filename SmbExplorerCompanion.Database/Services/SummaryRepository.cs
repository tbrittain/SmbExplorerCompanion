using Microsoft.EntityFrameworkCore;
using OneOf;
using OneOf.Types;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services;

public class SummaryRepository : ISummaryRepository
{
    private readonly SmbExplorerCompanionDbContext _context;
    private readonly IApplicationContext _applicationContext;

    public SummaryRepository(SmbExplorerCompanionDbContext context, IApplicationContext applicationContext)
    {
        _context = context;
        _applicationContext = applicationContext;
    }

    public async Task<OneOf<FranchiseSummaryDto, None, Exception>> GetFranchiseSummaryAsync(CancellationToken cancellationToken = default)
    {
        var franchiseId = _applicationContext.SelectedFranchiseId!.Value;

        try
        {
            var franchiseSeasons = await _context.Seasons
                .Where(x => x.FranchiseId == franchiseId)
                .OrderByDescending(x => x.Number)
                .ToListAsync(cancellationToken: cancellationToken);

            // This indicates that no data has been imported yet
            if (!franchiseSeasons.Any()) return new None();

            var franchiseSummaryDto = new FranchiseSummaryDto
            {
                NumSeasons = franchiseSeasons.Count
            };

            var playersIQueryable = _context.Players
                .Where(x => x.FranchiseId == franchiseId);

            var numPlayers = await playersIQueryable
                .CountAsync(cancellationToken: cancellationToken);

            franchiseSummaryDto.NumPlayers = numPlayers;

            var mostRecentSeason = franchiseSeasons
                .OrderByDescending(x => x.Number)
                .First();

            franchiseSummaryDto.MostRecentSeasonNumber = mostRecentSeason.Number;

            var numHallOfFamers = await playersIQueryable
                .Where(x => x.IsHallOfFamer)
                .CountAsync(cancellationToken: cancellationToken);

            franchiseSummaryDto.NumHallOfFamers = numHallOfFamers;

            var mostRecentChampionTeam = await _context.SeasonTeamHistory
                .Include(x => x.Team)
                .Include(x => x.TeamNameHistory)
                .Where(x => x.Team.FranchiseId == franchiseId)
                .Where(x => x.SeasonId == mostRecentSeason.Id)
                .Where(x => x.ChampionshipWinner != null)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (mostRecentChampionTeam is not null)
            {
                franchiseSummaryDto.MostRecentChampionTeamId = mostRecentChampionTeam.Team.Id;
                franchiseSummaryDto.MostRecentChampionTeamName = mostRecentChampionTeam.TeamNameHistory.Name;
            }

            // Get some top player leaders
            var topHomeRuns = await playersIQueryable
                .Select(x => new PlayerLeaderSummaryDto
                {
                    PlayerId = x.Id,
                    PlayerName = $"{x.FirstName} {x.LastName}",
                    StatName = "Home Runs",
                    StatValue = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.HomeRuns)),
                })
                .OrderByDescending(x => x.StatValue)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (topHomeRuns is not null)
            {
                franchiseSummaryDto.TopHomeRuns = topHomeRuns;
            }

            var topHits = await playersIQueryable
                .Select(x => new PlayerLeaderSummaryDto
                {
                    PlayerId = x.Id,
                    PlayerName = $"{x.FirstName} {x.LastName}",
                    StatName = "Hits",
                    StatValue = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.Hits)),
                })
                .OrderByDescending(x => x.StatValue)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (topHits is not null)
            {
                franchiseSummaryDto.TopHits = topHits;
            }

            var topRunsBattedIn = await playersIQueryable
                .Select(x => new PlayerLeaderSummaryDto
                {
                    PlayerId = x.Id,
                    PlayerName = $"{x.FirstName} {x.LastName}",
                    StatName = "Runs Batted In",
                    StatValue = x.PlayerSeasons.Sum(y => y.BattingStats.Sum(z => z.RunsBattedIn)),
                })
                .OrderByDescending(x => x.StatValue)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (topRunsBattedIn is not null)
            {
                franchiseSummaryDto.TopRunsBattedIn = topRunsBattedIn;
            }

            var topWins = await playersIQueryable
                .Select(x => new PlayerLeaderSummaryDto
                {
                    PlayerId = x.Id,
                    PlayerName = $"{x.FirstName} {x.LastName}",
                    StatName = "Wins",
                    StatValue = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Wins)),
                })
                .OrderByDescending(x => x.StatValue)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (topWins is not null)
            {
                franchiseSummaryDto.TopWins = topWins;
            }

            var topSaves = await playersIQueryable
                .Select(x => new PlayerLeaderSummaryDto
                {
                    PlayerId = x.Id,
                    PlayerName = $"{x.FirstName} {x.LastName}",
                    StatName = "Saves",
                    StatValue = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Saves)),
                })
                .OrderByDescending(x => x.StatValue)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (topSaves is not null)
            {
                franchiseSummaryDto.TopSaves = topSaves;
            }

            var topStrikeouts = await playersIQueryable
                .Select(x => new PlayerLeaderSummaryDto
                {
                    PlayerId = x.Id,
                    PlayerName = $"{x.FirstName} {x.LastName}",
                    StatName = "Strikeouts",
                    StatValue = x.PlayerSeasons.Sum(y => y.PitchingStats.Sum(z => z.Strikeouts)),
                })
                .OrderByDescending(x => x.StatValue)
                .FirstOrDefaultAsync(cancellationToken: cancellationToken);

            if (topStrikeouts is not null)
            {
                franchiseSummaryDto.TopStrikeouts = topStrikeouts;
            }

            var rand = new Random();
            var randomPlayers = await playersIQueryable
                .OrderBy(x => rand.Next())
                .Take(6)
                .Select(x => new PlayerBaseDto
                {
                    PlayerId = x.Id,
                    PlayerName = $"{x.FirstName} {x.LastName}",
                })
                .ToListAsync(cancellationToken: cancellationToken);
            
            franchiseSummaryDto.RandomPlayers = randomPlayers;

            return franchiseSummaryDto;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}