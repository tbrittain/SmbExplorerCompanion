using SmbExplorerCompanion.Core.Entities.Players;

namespace SmbExplorerCompanion.WPF.Models.Players;

public class SimilarPlayer : PlayerBase
{
    public double SimilarityScore { get; set; }

    public string SimilarityScoreString => $"Score: {SimilarityScore:F1}";
}

public static class SimilarPlayerExtensions
{
    public static SimilarPlayer FromCore(this SimilarPlayerDto x)
    {
        return new SimilarPlayer
        {
            PlayerId = x.PlayerId,
            PlayerName = x.PlayerName,
            SimilarityScore = x.SimilarityScore
        };
    }
}