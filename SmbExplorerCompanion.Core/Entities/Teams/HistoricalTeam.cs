namespace SmbExplorerCompanion.Core.Entities.Teams;

public class HistoricalTeam
{
    public int Id { get; set; }
    public string CurrentName { get; set; } = default!;
    public int NumGames { get; set; }
    public int NumWins { get; set; }
    public int NumLosses { get; set; }
    public double WinLossPct => (double)NumWins / NumGames;
    public int GamesOver500 => NumWins - NumLosses;
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