namespace SmbExplorerCompanion.Core.Entities;

public class ConferenceDto
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public List<DivisionDto> Divisions { get; set; } = new();
}