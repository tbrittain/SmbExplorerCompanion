using SmbExplorerCompanion.Shared.Enums;

namespace SmbExplorerCompanion.Core.Entities;

public class PlayerHistoryDto
{
    public int Id { get; set; }
    public int SeasonId { get; set; }
    public int SeasonNum { get; set; }
    public int Age { get; set; }
    public int Salary { get; set; }
    public Position SecondaryPosition { get; set; }
}