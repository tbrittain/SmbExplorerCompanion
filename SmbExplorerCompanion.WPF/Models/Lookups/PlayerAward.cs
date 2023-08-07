namespace SmbExplorerCompanion.WPF.Models.Lookups;

public class PlayerAward
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public bool IsBuiltIn { get; set; }
    public int Importance { get; set; }
    public bool OmitFromGroupings { get; set; }
    public bool IsBattingAward { get; set; }
    public bool IsPitchingAward { get; set; }
    public bool IsFieldingAward { get; set; }
    public bool IsPlayoffAward { get; set; }
}