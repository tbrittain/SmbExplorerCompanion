namespace SmbExplorerCompanion.Core.Entities.Players;

public abstract class PlayerBaseDto
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
}