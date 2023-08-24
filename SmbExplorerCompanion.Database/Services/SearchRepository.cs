using System.Text;
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

            return playerResults;
        }
        catch (Exception e)
        {
            return e;
        }
    }
}