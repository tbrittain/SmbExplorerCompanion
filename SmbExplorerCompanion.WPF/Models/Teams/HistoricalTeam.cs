using SmbExplorerCompanion.Core.Entities.Teams;

namespace SmbExplorerCompanion.WPF.Models.Teams;

public class HistoricalTeam
{
    public int TeamId { get; set; }
    public int? SeasonTeamId { get; set; }
    public string CurrentTeamName { get; set; } = default!;
    public int NumGames { get; set; }
    public int NumRegularSeasonWins { get; set; }
    public int NumRegularSeasonLosses { get; set; }
    public double WinLossPct { get; set; }
    public int GamesOver500 { get; set; }
    public int NumPlayoffWins { get; set; }
    public int NumPlayoffLosses { get; set; }
    public int? WinDiffFromPrevSeason { get; set; }
    // if positive, prepend a plus sign
    public string? DisplayWinDiffFromPrevSeason => WinDiffFromPrevSeason switch
    {
        null => null,
        < 0 => WinDiffFromPrevSeason.ToString(),
        _ => $"+{WinDiffFromPrevSeason}"
    };
    public int ChampionshipDroughtSeasons { get; set; }
    public int NumDivisionsWon { get; set; }
    public int NumConferenceTitles { get; set; }
    public int NumChampionships { get; set; }
    public int NumPlayoffAppearances { get; set; }
    public int NumPlayers { get; set; }
    public int NumHallOfFamers { get; set; }
    public int NumRunsScored { get; set; }
    public int NumRunsAllowed { get; set; }
    public int NumAtBats { get; set; }
    public int NumHits { get; set; }
    public int NumHomeRuns { get; set; }
    public double BattingAverage { get; set; }
    public double EarnedRunAverage { get; set; }
}

public static class HistoricalTeamExtensions
{
    public static HistoricalTeam FromCore(this HistoricalTeamDto x)
    {
        return new HistoricalTeam
        {
            TeamId = x.TeamId,
            SeasonTeamId = x.SeasonTeamId,
            NumGames = x.NumGames,
            NumRegularSeasonWins = x.NumRegularSeasonWins,
            NumRegularSeasonLosses = x.NumRegularSeasonLosses,
            WinLossPct = x.WinLossPct,
            GamesOver500 = x.GamesOver500,
            NumPlayoffWins = x.NumPlayoffWins,
            NumPlayoffLosses = x.NumPlayoffLosses,
            WinDiffFromPrevSeason = x.WinDiffFromPrevSeason,
            ChampionshipDroughtSeasons = x.ChampionshipDroughtSeasons,
            NumDivisionsWon = x.NumDivisionsWon,
            NumConferenceTitles = x.NumConferenceTitles,
            NumChampionships = x.NumChampionships,
            NumPlayoffAppearances = x.NumPlayoffAppearances,
            NumPlayers = x.NumPlayers,
            NumHallOfFamers = x.NumHallOfFamers,
            NumRunsScored = x.NumRunsScored,
            NumRunsAllowed = x.NumRunsAllowed,
            NumAtBats = x.NumAtBats,
            NumHits = x.NumHits,
            NumHomeRuns = x.NumHomeRuns,
            BattingAverage = x.BattingAverage,
            EarnedRunAverage = x.EarnedRunAverage,
            CurrentTeamName = x.CurrentTeamName
        };
    }
}