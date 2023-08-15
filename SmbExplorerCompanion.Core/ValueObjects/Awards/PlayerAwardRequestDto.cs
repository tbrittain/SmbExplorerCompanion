namespace SmbExplorerCompanion.Core.ValueObjects.Awards;

public record PlayerAwardRequestDto
{
    public int PlayerId { get; set; }
    public int AwardId { get; set; }
}