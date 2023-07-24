namespace SmbExplorerCompanion.Core.Entities;

public class SeasonTeamHistoryDto
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public int SeasonNum { get; set; }
    public long Budget { get; set; }
    public long Payroll { get; set; }
    public long Surplus { get; set; }
    public long SurplusPerGame { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double GamesBehind { get; set; }
    public double WinPercentage { get; set; }
    public double PythagoreanWinPercentage { get; set; }
    public int ExpectedWins { get; set; }
    public int ExpectedLosses { get; set; }
    public int TotalPower { get; set; }
    public int TotalContact { get; set; }
    public int TotalSpeed { get; set; }
    public int TotalFielding { get; set; }
    public int TotalArm { get; set; }
    public int TotalVelocity { get; set; }
    public int TotalJunk { get; set; }
    public int TotalAccuracy { get; set; }
    public bool MadePlayoffs { get; set; }
    public bool WonChampionship { get; set; }
    public bool WonDivision => GamesBehind == 0;
    public int PlayoffSeed { get; set; }
    public int PlayoffWins { get; set; }
    public int PlayoffLosses { get; set; }
    public int PlayoffRunDifferential { get; set; }
    public int PlayoffRunsFor { get; set; }
    public int PlayoffRunsAgainst { get; set; }
}