namespace SmbExplorerCompanion.Core.Entities.Teams;

public class TeamPlayoffRoundResultDto
{
    public PlayoffRound Round { get; set; }
    public bool WonSeries { get; set; }
    public int SeriesNumber { get; set; }
    public string OpponentTeamName { get; set; } = default!;
    public int OpponentSeasonTeamId { get; set; }
    public int NumWins { get; set; }
    public int NumLosses { get; set; }
}