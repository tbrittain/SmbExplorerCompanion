namespace SmbExplorerCompanion.Core.ValueObjects.Progress;

public record struct ImportProgress
{
    public Guid Id { get; init; }
    public string CsvFileName { get; init; }
    public int TotalRecords { get; init; }
    public int RecordNumber { get; init; }
}