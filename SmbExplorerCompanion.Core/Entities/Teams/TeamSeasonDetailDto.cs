using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.Core.Entities.Teams;

public class TeamSeasonDetailDto : TeamBaseDto
{
    public string TeamName { get; set; } = default!;
    public string DivisionName { get; set; } = default!;
    public string ConferenceName { get; set; } = default!;
    public int SeasonNum { get; set; }
    public int Budget { get; set; }
    public int Payroll { get; set; }
    public int Surplus { get; set; }
    public int SurplusPerGame { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int RunDifferential { get; set; }
    public int RunsFor { get; set; }
    public int RunsAgainst { get; set; }
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
    public int? PlayoffSeed { get; set; }
    public int? PlayoffWins { get; set; }
    public int? PlayoffLosses { get; set; }
    public int? PlayoffRunsScored { get; set; }
    public int? PlayoffRunsAllowed { get; set; }
    public bool WonConference { get; set; }
    public bool WonDivision => GamesBehind == 0;
    public bool WonChampionship { get; set; }

    public List<PlayerPitchingSeasonDto> RegularSeasonPitching { get; set; } = new();
    public List<PlayerPitchingSeasonDto> PlayoffPitching { get; set; } = new();
    public List<PlayerBattingSeasonDto> RegularSeasonBatting { get; set; } = new();
    public List<PlayerBattingSeasonDto> PlayoffBatting { get; set; } = new();
}