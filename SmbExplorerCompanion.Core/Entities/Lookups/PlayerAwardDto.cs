namespace SmbExplorerCompanion.Core.Entities.Lookups;

public class PlayerAwardDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string OriginalName { get; set; } = default!;
    public bool IsBuiltIn { get; set; }
    public int Importance { get; set; }
    public bool OmitFromGroupings { get; set; }
    public bool IsBattingAward { get; set; }
    public bool IsPitchingAward { get; set; }
    public bool IsFieldingAward { get; set; }
    public bool IsPlayoffAward { get; set; }
    public bool IsUserAssignable { get; set; }
}