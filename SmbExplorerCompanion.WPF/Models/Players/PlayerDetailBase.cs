namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerDetailBase : PlayerBase
{
    public bool IsPitcher { get; set; }
    public int TotalSalary { get; set; }
    public int BatHandednessId { get; set; }
    public int ThrowHandednessId { get; set; }
    public int PrimaryPositionId { get; set; }
    public int? PitcherRoleId { get; set; }
    public int? ChemistryId { get; set; }
    public double WeightedOpsPlusOrEraMinus { get; set; }
    public string DisplayPrimaryPosition => IsPitcher ? $"{PrimaryPosition} ({PitcherRole})" : PrimaryPosition;
}