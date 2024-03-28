using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Models.Players;

namespace SmbExplorerCompanion.WPF.Models.Teams;

public class TeamTopPlayerHistory : PlayerBase
{
    public int NumSeasonsWithTeam { get; set; }
    public List<int> SeasonNumbers { get; set; } = new();

    public string Seasons
    {
        get
        {
            var seasons = SeasonNumbers.OrderBy(x => x).ToList();
            var ranges = new List<string>();
            for (var i = 0; i < seasons.Count; i++)
            {
                var start = seasons[i];

                while (i < seasons.Count - 1 && seasons[i] + 1 == seasons[i + 1]) i++;

                var end = seasons[i];

                // If start and end are the same, it's just a single season, otherwise it's a range.
                if (start == end) ranges.Add(start.ToString());
                else ranges.Add($"{start}-{end}");
            }

            return string.Join(", ", ranges);
        }
    }

    public bool IsPitcher { get; set; }
    public string PlayerPosition { get; set; } = string.Empty;
    public double AverageOpsPlus { get; set; }
    public double AverageEraMinus { get; set; }
    public double AverageOpsPlusOrEraMinus => IsPitcher ? AverageEraMinus : AverageOpsPlus;
    public double WeightedOpsPlusOrEraMinus { get; set; }
    public ObservableCollection<PlayerAwardBase> Awards { get; set; } = new();
    public string? DisplayAwards => Awards.GetFormattedAwards(isSeason: false);
}

public static class TeamTopPlayerHistoryExtensions
{
    public static TeamTopPlayerHistory FromCore(this TeamTopPlayerHistoryDto x)
    {
        return new TeamTopPlayerHistory
        {
            PlayerId = x.PlayerId,
            PlayerName = x.PlayerName,
            NumSeasonsWithTeam = x.NumSeasonsWithTeam,
            SeasonNumbers = x.SeasonNumbers,
            IsPitcher = x.IsPitcher,
            PlayerPosition = x.PlayerPosition,
            AverageOpsPlus = x.AverageOpsPlus,
            AverageEraMinus = x.AverageEraMinus,
            WeightedOpsPlusOrEraMinus = x.WeightedOpsPlusOrEraMinus,
            Awards = x.Awards.Select(y => y.FromCore()).ToObservableCollection()
        };
    }
}