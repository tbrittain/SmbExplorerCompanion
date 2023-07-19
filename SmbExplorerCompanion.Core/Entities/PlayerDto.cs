using SmbExplorerCompanion.Shared.Enums;

namespace SmbExplorerCompanion.Core.Entities;

public class PlayerDto
{
    public int Id { get; set; }
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string FullName => $"{FirstName} {LastName}";
    public bool IsHallOfFamer { get; set; }
    public Handedness BatHandedness { get; set; }
    public Handedness ThrowHandedness { get; set; }
    public Position PrimaryPosition { get; set; }
    public Chemistry Chemistry { get; set; }
    public List<PlayerHistoryDto> PlayerHistory { get; set; } = new();
}