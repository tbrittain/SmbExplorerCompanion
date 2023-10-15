namespace SmbExplorerCompanion.Core.Entities.Teams;

public record TeamScheduleBreakdownDto(int TeamHistoryId,
    string TeamName,
    int OpponentTeamHistoryId,
    string OpponentTeamName,
    int Day,
    int GlobalGameNumber,
    int TeamScore,
    int OpponentTeamScore);