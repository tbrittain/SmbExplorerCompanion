using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Extensions;

public static class AwardsExtensions
{
    public static string? GetFormattedAwards(this IEnumerable<PlayerAwardBase> awards, bool isSeason = true)
    {
        var awardsList = awards.ToList();
        if (!awardsList.Any()) return null;

        if (!isSeason)
        {
            awards = awardsList.Where(x => !x.OmitFromGroupings).ToList();
            if (!awards.Any()) return null;
        }

        var sb = new StringBuilder();

        var groupings = awardsList
            .GroupBy(x => x.Id)
            .OrderBy(x => x.First().Importance)
            .ThenBy(x => x.First().Name)
            .ToList();

        var lastGrouping = groupings.Last();
        foreach (var grouping in groupings)
        {
            if (!isSeason)
            {
                sb.Append($"{grouping.Count()}x ");
            }
            sb.Append($"{grouping.First().Name}");

            if (grouping.Key != lastGrouping.Key)
            {
                sb.Append(", ");
            }
        }

        return sb.ToString();
    }
}