using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Lookups;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerOverview : PlayerCareerBase
{
    public string BatHandedness { get; set; } = string.Empty;
    public string ThrowHandedness { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    public string? PitcherRole { get; set; }
    public string Chemistry { get; set; } = string.Empty;
    public int NumChampionships { get; set; }
    public string CurrentTeam { get; set; } = string.Empty;
    public int? CurrentTeamId { get; set; }
    public List<PlayerAward> Awards { get; set; } = new();

    public ObservableCollection<FormattedPlayerAward> AggregatedAwards
    {
        get
        {
            if (!Awards.Any()) return new ObservableCollection<FormattedPlayerAward>();

            var awards = Awards
                .Where(x => !x.OmitFromGroupings)
                .GroupBy(x => x.Id)
                .OrderBy(x => x.First().Importance)
                .Select(x => x.GetFormattedPlayerAward())
                .ToObservableCollection();

            return awards;
        }
    }

    public PlayerBattingCareer CareerBatting { get; init; } = null!;
    public PlayerPitchingCareer CareerPitching { get; init; } = null!;
    public ObservableCollection<PlayerBattingCareer> PlayerCareerBatting => new() {CareerBatting};
    public ObservableCollection<PlayerPitchingCareer> PlayerCareerPitching => new() {CareerPitching};
    public ObservableCollection<PlayerSeasonBatting> PlayerSeasonBatting { get; set; } = new();
    public ObservableCollection<PlayerSeasonBatting> PlayerPlayoffBatting { get; set; } = new();
    public ObservableCollection<PlayerSeasonPitching> PlayerSeasonPitching { get; set; } = new();
    public ObservableCollection<PlayerSeasonPitching> PlayerPlayoffPitching { get; set; } = new();
    public ObservableCollection<PlayerGameStatOverview> GameStats { get; set; } = new();

    public string DisplayPosition
    {
        get
        {
            var sb = new StringBuilder(PrimaryPosition);

            if (PitcherRole is not null)
            {
                sb.Append($" ({PitcherRole})");
            }

            return sb.ToString();
        }
    }
}

public static class PlayerOverviewExtensions
{
    public static PlayerOverview FromCore(this PlayerOverviewDto x, LookupSearchService lss)
    {
        var overview = new PlayerOverview
        {
            PlayerId = x.PlayerId,
            PlayerName = x.PlayerName,
            IsPitcher = x.IsPitcher,
            TotalSalary = x.TotalSalary,
            BatHandednessId = x.BatHandednessId,
            ThrowHandednessId = x.ThrowHandednessId,
            PrimaryPositionId = x.PrimaryPositionId,
            PitcherRoleId = x.PitcherRoleId,
            ChemistryId = x.ChemistryId,
            WeightedOpsPlusOrEraMinus = x.WeightedOpsPlusOrEraMinus,
            StartSeasonNumber = x.StartSeasonNumber,
            EndSeasonNumber = x.EndSeasonNumber,
            IsRetired = x.IsRetired,
            Age = x.Age,
            RetiredCurrentAge = x.RetiredCurrentAge,
            IsHallOfFamer = x.IsHallOfFamer,
            NumSeasons = x.NumSeasons,
            NumChampionships = x.NumChampionships,
            CurrentTeamId = x.CurrentTeamId,
            CurrentTeam = x.CurrentTeam,
            Awards = x.Awards.Select(y => y.FromCore()).ToList(),
            CareerBatting = x.CareerBatting.FromCore(lss),
            CareerPitching = x.CareerPitching.FromCore(),
            PlayerSeasonBatting = x.PlayerSeasonBatting.Select(y => y.FromCore()).ToObservableCollection(),
            PlayerSeasonPitching = x.PlayerSeasonPitching.Select(y => y.FromCore()).ToObservableCollection(),
            PlayerPlayoffBatting = x.PlayerPlayoffBatting.Select(y => y.FromCore()).ToObservableCollection(),
            PlayerPlayoffPitching = x.PlayerPlayoffPitching.Select(y => y.FromCore()).ToObservableCollection(),
            GameStats = x.GameStats.Select(y => y.FromCore()).ToObservableCollection(),
            DisplayPrimaryPosition = x.GetDisplayPrimaryPosition(lss)
        };
        
        // TODO: hydrate the lookups with the cache here

        return overview;
    }
}