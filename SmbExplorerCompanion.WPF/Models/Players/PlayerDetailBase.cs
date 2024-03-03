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
    public required string DisplayPrimaryPosition { get; set; }
}

public static class PlayerDetailBaseExtensions
{
    public static string GetDisplayPrimaryPosition(this PlayerDetailBaseDto original, LookupSearchService lss)
    {
        var isPitcher = original.IsPitcher;
        var primaryPosition = lss.GetPositionById(original.PrimaryPositionId).Result;
        var pitcherRole = original.PitcherRoleId.HasValue 
            ? lss.GetPitcherRoleById(original.PitcherRoleId.Value).Result 
            : null;

        return isPitcher ? $"{primaryPosition!.Name} ({pitcherRole!.Name})" : primaryPosition!.Name;
    }
}