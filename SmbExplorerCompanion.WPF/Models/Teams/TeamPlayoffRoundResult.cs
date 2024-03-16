using System;
using System.Text;
using SmbExplorerCompanion.Core.Entities.Teams;

namespace SmbExplorerCompanion.WPF.Models.Teams;

public class TeamPlayoffRoundResult
{
    public PlayoffRound Round { get; set; }
    public bool WonSeries { get; set; }
    public int SeriesNumber { get; set; }
    public string OpponentTeamName { get; set; } = default!;
    public int OpponentSeasonTeamId { get; set; }
    public int NumWins { get; set; }
    public int NumLosses { get; set; }
    
    private string DisplayRound => Round switch
    {
        PlayoffRound.WildCard => "Wild Card",
        PlayoffRound.DivisionSeries => "Divisional",
        PlayoffRound.ConferenceSeries => "Conference",
        PlayoffRound.ChampionshipSeries => "Championship",
        _ => throw new ArgumentOutOfRangeException()
    };

    public string DisplayPlayoffRoundResult
    {
        get
        {
            var sb = new StringBuilder();
            sb.Append(WonSeries ? "Won" : "Lost");
            sb.Append($" in the {DisplayRound} Series");
            sb.Append($" against the {OpponentTeamName}");
            sb.Append($" {NumWins}-{NumLosses}");
            return sb.ToString();
        }
    }
}

public static class TeamPlayoffRoundResultExtensions
{
    public static TeamPlayoffRoundResult FromCore(this TeamPlayoffRoundResultDto x)
    {
        return new TeamPlayoffRoundResult
        {
            Round = x.Round,
            WonSeries = x.WonSeries,
            SeriesNumber = x.SeriesNumber,
            OpponentTeamName = x.OpponentTeamName,
            OpponentSeasonTeamId = x.OpponentSeasonTeamId,
            NumWins = x.NumWins,
            NumLosses = x.NumLosses
        };
    }
}