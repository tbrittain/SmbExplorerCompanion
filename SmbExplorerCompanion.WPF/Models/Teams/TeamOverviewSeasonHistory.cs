using SmbExplorerCompanion.Core.Entities.Teams;

namespace SmbExplorerCompanion.WPF.Models.Teams;

public class TeamOverviewSeasonHistory
{
    public int SeasonTeamId { get; set; }
    public int SeasonNumber { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public string ConferenceName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public double GamesBehind { get; set; }
    public int? PlayoffWins { get; set; }
    public int? PlayoffLosses { get; set; }
    public int? PlayoffSeed { get; set; }
    public bool WonConference { get; set; }
    public bool WonChampionship { get; set; }
}

public static class TeamOverviewSeasonHistoryExtensions
{
    public static TeamOverviewSeasonHistory FromCore(this TeamOverviewSeasonHistoryDto x)
    {
        return new TeamOverviewSeasonHistory
        {
            SeasonTeamId = x.SeasonTeamId,
            SeasonNumber = x.SeasonNumber,
            DivisionName = x.DivisionName,
            ConferenceName = x.ConferenceName,
            TeamName = x.TeamName,
            Wins = x.Wins,
            Losses = x.Losses,
            GamesBehind = x.GamesBehind,
            PlayoffWins = x.PlayoffWins,
            PlayoffLosses = x.PlayoffLosses,
            PlayoffSeed = x.PlayoffSeed,
            WonConference = x.WonConference,
            WonChampionship = x.WonChampionship
        };
    }
}