namespace SmbExplorerCompanion.WPF.Models.Teams;

public record TeamScheduleBreakdown(int TeamHistoryId,
    string TeamName,
    int OpponentTeamHistoryId,
    string OpponentTeamName,
    int Day,
    int GlobalGameNumber,
    int TeamScore,
    int OpponentTeamScore,
    int WinsDelta);