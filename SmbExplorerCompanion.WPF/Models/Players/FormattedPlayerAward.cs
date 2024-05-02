using System.Windows.Media;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class FormattedPlayerAward
{
    public static readonly Brush BaseColor = Brushes.Silver;
    public static readonly Brush Importance1Color = Brushes.Gold;
    public static readonly Brush HallOfFameColor = Brushes.LightGreen;

    public string DisplayName { get; init; } = default!;
    public int Importance { get; init; }
    public bool FullWidth { get; init; }
    public required Brush Color { get; init; }
}