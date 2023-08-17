using SmbExplorerCompanion.WPF.Models.Lookups;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class FormattedPlayerAward
{
    public const string BaseColor = "#d1d1d1";
    public const string Importance1Color = "#f5f5f5";
    public const string HallOfFameColor = "#fcc616";
    
    public string DisplayName { get; init; } = default!;
    public int Importance { get; init; }
    public bool FullWidth { get; init; }
    public string Color { get; init; } = default!;
}