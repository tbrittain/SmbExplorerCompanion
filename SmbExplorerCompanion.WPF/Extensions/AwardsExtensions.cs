using System.Collections.Generic;
using System.Linq;
using System.Text;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;

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
            if (!isSeason && grouping.First().Name != "Hall of Fame")
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

    public static FormattedPlayerAward GetFormattedPlayerAward(this IEnumerable<PlayerAward> groupedAwards)
    {
        var awards = groupedAwards.ToList();
        var count = awards.Count;
        var award = awards.First();
        
        var awardName = award.Name;

        return new FormattedPlayerAward
        {
            DisplayName = count > 1 ? $"{count}x {awardName}" : awardName,
            Importance = award.Importance,
            FullWidth = award.OriginalName == "Hall of Fame",
            Color = award.OriginalName switch
            {
                "Hall of Fame" => FormattedPlayerAward.HallOfFameColor,
                _ => award.Importance switch
                {
                    <= 1 => FormattedPlayerAward.Importance1Color,
                    _ => FormattedPlayerAward.BaseColor,
                }
            }
        };
    }
}