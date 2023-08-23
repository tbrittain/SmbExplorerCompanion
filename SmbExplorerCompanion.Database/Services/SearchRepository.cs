using Microsoft.EntityFrameworkCore;
using OneOf;
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

    public async Task<OneOf<IEnumerable<SearchResultDto>, Exception>> Search(string query, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(query)) return new List<SearchResultDto>();

        try
        {
            var matchingPlayers = await _context.Players
                .Include(x => x.PlayerSeasons)
                .ThenInclude(x => x.Season)
                .Where(x => x.FirstName.Contains(query) ||
                            x.LastName.Contains(query) ||
                            (x.FirstName + " " + x.LastName).Contains(query))
                .OrderBy(x => x.LastName)
                .Take(10)
                .ToListAsync(cancellationToken: cancellationToken);

            var playerResults = matchingPlayers
                .Select(x =>
                {
                    var firstSeason = x.PlayerSeasons.OrderBy(y => y.Id).First().Season.Number;
                    var lastSeason = x.PlayerSeasons.OrderBy(y => y.Id).Last().Season.Number;
                    var seasonRange = firstSeason == lastSeason ? firstSeason.ToString() : $"{firstSeason}-{lastSeason}";

                    return new SearchResultDto
                    {
                        Type = SearchResultType.Player,
                        Name = $"{x.FirstName} {x.LastName}",
                        Description = $"{seasonRange} - {x.PlayerSeasons.Count} seasons",
                        Id = x.Id
                    };
                })
                .ToList();

            return playerResults;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}