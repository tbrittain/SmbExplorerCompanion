namespace SmbExplorerCompanion.WPF.Models.Lookups;

public class PlayerAward : PlayerAwardBase
{
    public bool IsBuiltIn { get; set; }
    public bool IsBattingAward { get; set; }
    public bool IsPitchingAward { get; set; }
    public bool IsFieldingAward { get; set; }
    public bool IsPlayoffAward { get; set; }
}