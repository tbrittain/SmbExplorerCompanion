namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerDetailBaseDto : PlayerBaseDto
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
    public int BatHandednessId { get; set; }
    public int ThrowHandednessId { get; set; }
    public int PrimaryPositionId { get; set; }
    public int? ChemistryId { get; set; }
    public int? PitcherRoleId { get; set; }
}