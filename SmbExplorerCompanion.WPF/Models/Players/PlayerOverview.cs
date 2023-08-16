using System.Collections.ObjectModel;
using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerOverview
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public bool IsPitcher { get; set; }
    public int TotalSalary { get; set; }
    public string BatHandedness { get; set; } = string.Empty;
    public string ThrowHandedness { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    public string? PitcherRole { get; set; }
    public string Chemistry { get; set; } = string.Empty;
    public double WeightedOpsPlusOrEraMinus { get; set; }
    public int StartSeasonNumber { get; set; }
    public int EndSeasonNumber { get; set; }
    public bool IsRetired { get; set; }
    public int NumSeasons { get; set; }
    public int NumChampionships { get; set; }
    public bool IsHallOfFamer { get; set; }
    public string CurrentTeam { get; set; } = string.Empty;
    public int? CurrentTeamId { get; set; }
    public ObservableCollection<PlayerAwardBase> Awards { get; set; } = new();

    public PlayerBattingCareer CareerBatting { get; init; } = null!;
    public PlayerPitchingCareer CareerPitching { get; init; } = null!;
    public ObservableCollection<PlayerBattingCareer> PlayerCareerBatting => new() { CareerBatting };
    public ObservableCollection<PlayerPitchingCareer> PlayerCareerPitching => new() { CareerPitching };
    public ObservableCollection<PlayerSeasonBatting> PlayerSeasonBatting { get; set; } = new();
    public ObservableCollection<PlayerSeasonBatting> PlayerPlayoffBatting { get; set; } = new();
    public ObservableCollection<PlayerSeasonPitching> PlayerSeasonPitching { get; set; } = new();
    public ObservableCollection<PlayerSeasonPitching> PlayerPlayoffPitching { get; set; } = new();
    public ObservableCollection<PlayerGameStatOverview> GameStats { get; set; } = new();
}