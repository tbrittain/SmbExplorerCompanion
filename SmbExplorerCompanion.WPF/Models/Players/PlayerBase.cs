namespace SmbExplorerCompanion.WPF.Models.Players;

public abstract class PlayerBase
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
}