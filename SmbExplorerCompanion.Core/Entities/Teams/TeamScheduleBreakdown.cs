namespace SmbExplorerCompanion.Core.Entities.Teams;

public record TeamScheduleBreakdown(int TeamId, string TeamName, int OpponentTeamId, string OpponentTeamName, int Day, int GlobalGameNumber, int TeamScore, int OpponentTeamScore);