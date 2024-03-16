namespace SmbExplorerCompanion.WPF.Models.Players;

public class PlayerGameStatOverview
{
    public int SeasonNumber { get; set; }
    public int SeasonId { get; set; }
    public int Age { get; set; }
    public string TeamNames { get; set; } = string.Empty;
    public int Power { get; set; }
    public int Contact { get; set; }
    public int Speed { get; set; }
    public int Fielding { get; set; }
    public int? Arm { get; set; }
    public int? Velocity { get; set; }
    public int? Junk { get; set; }
    public int? Accuracy { get; set; }
    public int Salary { get; set; }
    public string? SecondaryPosition { get; set; } = string.Empty;
    public string Traits { get; set; } = string.Empty;
}