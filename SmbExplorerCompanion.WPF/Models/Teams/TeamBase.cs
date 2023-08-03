namespace SmbExplorerCompanion.WPF.Models.Teams;

public abstract class TeamBase
{
    public int TeamId { get; set; }
    public string CurrentTeamName { get; set; } = string.Empty;
}