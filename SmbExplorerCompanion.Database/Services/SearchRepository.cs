using System.Text;
using Microsoft.EntityFrameworkCore;
using SmbExplorerCompanion.Core.Entities.Search;
using SmbExplorerCompanion.Core.Interfaces;

namespace SmbExplorerCompanion.Database.Services;

public class SearchRepository : ISearchRepository
{
    private SmbExplorerCompanionDbContext _context;

    public SearchRepository(SmbExplorerCompanionDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<SearchResultDto>> Search(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<SearchResultDto>();

        var lowerQuery = query.ToLower();

        var matchingPlayers = await _context.Players
            .Include(x => x.PlayerSeasons)
            .ThenInclude(x => x.Season)
            .Where(x => x.FirstName.ToLower().Contains(lowerQuery) ||
                        x.LastName.ToLower().Contains(lowerQuery) ||
                        (x.FirstName.ToLower() + " " + x.LastName.ToLower()).Contains(lowerQuery))
            .OrderBy(x => x.LastName)
            .Take(10)
            .ToListAsync(cancellationToken: cancellationToken);

        var matchingTeams = await _context.TeamNameHistory
            .Include(x => x.SeasonTeamHistory)
            .ThenInclude(x => x.Team)
            .Where(x => x.Name.ToLower().Contains(lowerQuery))
            .OrderBy(x => x.Name)
            .Take(10)
            .ToListAsync(cancellationToken: cancellationToken);

        var playerResults = matchingPlayers
            .Select(x =>
            {
                var firstSeason = x.PlayerSeasons.OrderBy(y => y.Id).First().Season.Number;
                var lastSeason = x.PlayerSeasons.OrderBy(y => y.Id).Last().Season.Number;

                var seasonRange = firstSeason == lastSeason ? firstSeason.ToString() : $"{firstSeason}-{lastSeason}";

                var sb = new StringBuilder();
                sb.Append(x.PlayerSeasons.Count);
                sb.Append(" season");
                if (x.PlayerSeasons.Count > 1) sb.Append('s');
                sb.Append(" (");
                sb.Append(seasonRange);
                sb.Append(')');

                return new SearchResultDto
                {
                    Type = SearchResultType.Players,
                    Name = $"{x.FirstName} {x.LastName}",
                    Description = sb.ToString(),
                    Id = x.Id
                };
            })
            .ToList();

        var teamResults = matchingTeams
            .Select(x => new SearchResultDto
            {
                Type = SearchResultType.Teams,
                Name = x.Name,
                Id = x.SeasonTeamHistory.First().TeamId
            })
            .Distinct()
            .ToList();

        return playerResults.Concat(teamResults).ToList();
    }
}