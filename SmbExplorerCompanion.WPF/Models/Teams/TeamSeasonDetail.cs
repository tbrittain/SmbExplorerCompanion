using System.Collections.Generic;
using System.Linq;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.WPF.Models.Players;

namespace SmbExplorerCompanion.WPF.Models.Teams;

public class TeamSeasonDetail : TeamBase
{
    public string DivisionName { get; set; } = default!;
    public string ConferenceName { get; set; } = default!;
    public int SeasonNum { get; set; }
    public long Budget { get; set; }
    public long Payroll { get; set; }
    public long Surplus { get; set; }
    public double SurplusPerGame { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int RunDifferential { get; set; }
    public int RunsScored { get; set; }
    public int RunsAllowed { get; set; }
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
    public bool WonDivision { get; set; }
    public bool WonConference { get; set; }
    public bool WonChampionship { get; set; }
    public bool IncludesPlayoffData { get; set; }

    public List<TeamPlayoffRoundResult> PlayoffResults { get; set; } = new();

    public List<PlayerSeasonPitching> RegularSeasonPitching { get; set; } = new();
    public List<PlayerSeasonPitching> PlayoffPitching { get; set; } = new();
    public List<PlayerSeasonBatting> RegularSeasonBatting { get; set; } = new();
    public List<PlayerSeasonBatting> PlayoffBatting { get; set; } = new();

    public string TeamRecord => $"{Wins}-{Losses}, {WinPercentage:F3} W-L%";
}

public static class TeamSeasonDetailExtensions
{
    public static TeamSeasonDetail FromCore(this TeamSeasonDetailDto x)
    {
        return new TeamSeasonDetail
        {
            TeamId = x.TeamId,
            CurrentTeamName = x.CurrentTeamName,
            DivisionName = x.DivisionName,
            ConferenceName = x.ConferenceName,
            SeasonNum = x.SeasonNum,
            Budget = x.Budget,
            Payroll = x.Payroll,
            Surplus = x.Surplus,
            SurplusPerGame = x.SurplusPerGame,
            Wins = x.Wins,
            Losses = x.Losses,
            RunDifferential = x.RunDifferential,
            RunsScored = x.RunsScored,
            RunsAllowed = x.RunsAllowed,
            GamesBehind = x.GamesBehind,
            WinPercentage = x.WinPercentage,
            PythagoreanWinPercentage = x.PythagoreanWinPercentage,
            ExpectedWins = x.ExpectedWins,
            ExpectedLosses = x.ExpectedLosses,
            TotalPower = x.TotalPower,
            TotalContact = x.TotalContact,
            TotalSpeed = x.TotalSpeed,
            TotalFielding = x.TotalFielding,
            TotalArm = x.TotalArm,
            TotalVelocity = x.TotalVelocity,
            TotalJunk = x.TotalJunk,
            TotalAccuracy = x.TotalAccuracy,
            MadePlayoffs = x.MadePlayoffs,
            PlayoffSeed = x.PlayoffSeed,
            WonDivision = x.WonDivision,
            WonConference = x.WonConference,
            WonChampionship = x.WonChampionship,
            IncludesPlayoffData = x.IncludesPlayoffData,
            PlayoffResults = x.PlayoffResults
                .Select(y => y.FromCore())
                .ToList(),
            RegularSeasonPitching = x.RegularSeasonPitching
                .Select(y => y.FromCore())
                .ToList(),
            PlayoffPitching = x.PlayoffPitching
                .Select(y => y.FromCore())
                .ToList(),
            RegularSeasonBatting = x.RegularSeasonBatting
                .Select(y => y.FromCore())
                .ToList(),
            PlayoffBatting = x.PlayoffBatting
                .Select(y => y.FromCore())
                .ToList()
        };
    }
}