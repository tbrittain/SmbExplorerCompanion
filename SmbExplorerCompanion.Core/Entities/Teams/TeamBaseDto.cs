namespace SmbExplorerCompanion.Core.Entities.Teams;

public abstract class TeamBaseDto
{
    public int TeamId { get; set; }
    public string CurrentTeamName { get; set; } = string.Empty;
}