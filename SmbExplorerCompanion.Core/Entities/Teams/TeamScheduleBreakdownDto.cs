namespace SmbExplorerCompanion.Core.Entities.Teams;

public class DivisionScheduleBreakdownDto
{
    public List<HashSet<TeamScheduleBreakdownDto>> TeamScheduleBreakdowns { get; set; } = new();
}

public record TeamScheduleBreakdownDto(int TeamHistoryId,
    string TeamName,
    int OpponentTeamHistoryId,
    string OpponentTeamName,
    int Day,
    int GlobalGameNumber,
    int TeamScore,
    int OpponentTeamScore);