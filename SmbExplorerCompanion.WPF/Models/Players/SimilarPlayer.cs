namespace SmbExplorerCompanion.WPF.Models.Players;

public class SimilarPlayer
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = null!;
    public double SimilarityScore { get; set; }
    
    public string SimilarityScoreString => $"Score: {SimilarityScore:F1}";
}