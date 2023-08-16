namespace SmbExplorerCompanion.Core.Entities.Players;

public class PlayerBattingOverviewDto : PlayerBattingSeasonDto
{
    public int Age { get; set; }
    public int Games { get; set; }
    public int PlateAppearances { get; set; }
    public int CaughtStealing { get; set; }
    public int TotalBases { get; set; }
    public int Salary { get; set; }
    public string? SecondaryPosition { get; set; } = string.Empty;
    public string Traits { get; set; } = string.Empty;
}