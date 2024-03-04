using System.Linq;
using System.Threading.Tasks;
using SmbExplorerCompanion.Core.Entities.Teams;
using SmbExplorerCompanion.WPF.Models.Teams;

// ReSharper disable once CheckNamespace
namespace SmbExplorerCompanion.WPF.Services;

public partial class MappingService
{
    public Task<TeamSeasonDetail> FromCore(TeamSeasonDetailDto x)
    {
        return Task.FromResult(new TeamSeasonDetail
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
                .Select(async y => await FromCore(y))
                .Select(y => y.Result)
                .ToList(),
            PlayoffPitching = x.PlayoffPitching
                .Select(async y => await FromCore(y))
                .Select(y => y.Result)
                .ToList(),
            RegularSeasonBatting = x.RegularSeasonBatting
                .Select(async y => await FromCore(y))
                .Select(y => y.Result)
                .ToList(),
            PlayoffBatting = x.PlayoffBatting
                .Select(async y => await FromCore(y))
                .Select(y => y.Result)
                .ToList(),
        });
    }
}