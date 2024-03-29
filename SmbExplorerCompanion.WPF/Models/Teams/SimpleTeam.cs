﻿using SmbExplorerCompanion.Core.Entities.Teams;

namespace SmbExplorerCompanion.WPF.Models.Teams;

public class SimpleTeam
{
    public int SeasonTeamId { get; set; }
    public int SeasonId { get; set; }
    public int SeasonNumber { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public string ConferenceName { get; set; } = string.Empty;
    public int TeamId { get; set; }
    public string TeamName { get; set; } = string.Empty;
}

public static class SimpleTeamExtensions
{
    public static SimpleTeam FromCore(this TeamDto teamDto)
    {
        return new SimpleTeam
        {
            SeasonTeamId = teamDto.SeasonTeamId,
            SeasonId = teamDto.SeasonId,
            SeasonNumber = teamDto.SeasonNumber,
            DivisionName = teamDto.DivisionName,
            ConferenceName = teamDto.ConferenceName,
            TeamId = teamDto.TeamId,
            TeamName = teamDto.TeamName
        };
    }
}