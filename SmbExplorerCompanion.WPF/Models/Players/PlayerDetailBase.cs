using SmbExplorerCompanion.Core.Entities.Players;
using SmbExplorerCompanion.WPF.Services;

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
    public string DisplayPrimaryPosition { get; set; } = default!;
}

public static class PlayerDetailBaseExtensions
{
    public static string GetDisplayPrimaryPosition(string primaryPositionName, string? pitcherRoleName)
    {
        return !string.IsNullOrEmpty(pitcherRoleName) 
            ? $"{primaryPositionName} ({pitcherRoleName})" 
            : primaryPositionName;
    }
}