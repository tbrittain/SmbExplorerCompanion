namespace SmbExplorerCompanion.Core.Entities.Players;

public class SimilarPlayerDto
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = null!;
    public double SimilarityScore { get; set; }
}