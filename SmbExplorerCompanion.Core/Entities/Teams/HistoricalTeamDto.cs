namespace SmbExplorerCompanion.Core.Entities.Teams;

public class HistoricalTeamDto : TeamBaseDto
{
    public int? SeasonTeamId { get; set; }
    public int NumGames { get; set; }
    public int NumRegularSeasonWins { get; set; }
    public int NumRegularSeasonLosses { get; set; }
    public double WinLossPct => (double) NumRegularSeasonWins / (NumGames - NumPlayoffGames);
    public int GamesOver500 => NumRegularSeasonWins - NumRegularSeasonLosses;
    private int NumPlayoffGames => NumPlayoffWins + NumPlayoffLosses;
    public int NumPlayoffWins { get; set; }
    public int NumPlayoffLosses { get; set; }
    public int? WinDiffFromPrevSeason { get; set; }
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