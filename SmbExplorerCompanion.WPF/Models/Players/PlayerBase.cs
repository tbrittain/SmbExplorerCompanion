namespace SmbExplorerCompanion.WPF.Models.Players;

public abstract class PlayerBase
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public bool IsPitcher { get; set; }
    public int TotalSalary { get; set; }
    public string BatHandedness { get; set; } = string.Empty;
    public string ThrowHandedness { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    public string? PitcherRole { get; set; }
    public string Chemistry { get; set; } = string.Empty;
    public double WeightedOpsPlusOrEraMinus { get; set; }
    public string DisplayPrimaryPosition => IsPitcher ? $"{PrimaryPosition} ({PitcherRole})" : PrimaryPosition;
}