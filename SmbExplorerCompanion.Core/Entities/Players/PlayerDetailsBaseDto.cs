namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerDetailsBaseDto : PlayerBaseDto
{
    public bool IsPitcher { get; set; }
    public int TotalSalary { get; set; }
    public string BatHandedness { get; set; } = string.Empty;
    public string ThrowHandedness { get; set; } = string.Empty;
    public string PrimaryPosition { get; set; } = string.Empty;
    public string? PitcherRole { get; set; }
    public string Chemistry { get; set; } = string.Empty;
    /// <summary>
    ///  We can kind of think of this as a proxy for WAR, but it's not quite the same
    /// </summary>
    public double WeightedOpsPlusOrEraMinus { get; set; }
}