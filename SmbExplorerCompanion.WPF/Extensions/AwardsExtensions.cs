using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Extensions;

public static class AwardsExtensions
{
    public static string? GetFormattedAwards(this List<PlayerAwardBase> awards, bool isSeason = true)
    {
        if (!awards.Any()) return null;

        if (!isSeason)
        {
            awards = awards.Where(x => !x.OmitFromGroupings).ToList();
            if (!awards.Any()) return null;
        }

        var sb = new StringBuilder();

        var groupings = awards
            .GroupBy(x => x.Id)
            .OrderByDescending(x => x.First().Importance)
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