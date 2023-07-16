namespace SmbExplorerCompanion.Database.Entities;

public class SeasonTeamHistory
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public virtual Season Season { get; set; } = default!;
    public int TeamId { get; set; }
    public virtual Team Team { get; set; } = default!;
    public long Budget { get; set; }
    public long Payroll { get; set; }
    public double PayrollPerGame { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double GamesBehind { get; set; }
    public int RunsScored { get; set; }
    public int RunsAllowed { get; set; }
    public int TotalPower { get; set; }
    public int TotalContact { get; set; }
    public int TotalSpeed { get; set; }
    public int TotalFielding { get; set; }
    public int TotalArm { get; set; }
    public int TotalVelocity { get; set; }
    public int TotalJunk { get; set; }
    public int TotalAccuracy { get; set; }
    public int? PlayoffSeed { get; set; }
    public int? PlayoffWins { get; set; }
    public int? PlayoffLosses { get; set; }
    public int? PlayoffRunsScored { get; set; }
    public int? PlayoffRunsAllowed { get; set; }

    public virtual ICollection<PlayerTeamHistory> PlayerTeamHistory { get; set; } = new HashSet<PlayerTeamHistory>();

    public int TeamDivisionHistoryId { get; set; }
    public virtual TeamDivisionHistory TeamDivisionHistory { get; set; } = default!;

    public int TeamNameHistoryId { get; set; }
    public virtual TeamNameHistory TeamNameHistory { get; set; } = default!;

    public virtual ICollection<TeamSeasonSchedule> HomeSeasonSchedule { get; set; } = new HashSet<TeamSeasonSchedule>();

    public virtual ICollection<TeamSeasonSchedule> AwaySeasonSchedule { get; set; } = new HashSet<TeamSeasonSchedule>();

    public virtual ICollection<TeamPlayoffSchedule> HomePlayoffSchedule { get; set; } =
        new HashSet<TeamPlayoffSchedule>();

    public virtual ICollection<TeamPlayoffSchedule> AwayPlayoffSchedule { get; set; } =
        new HashSet<TeamPlayoffSchedule>();

    public virtual ChampionshipWinner? ChampionshipWinner { get; set; }
}