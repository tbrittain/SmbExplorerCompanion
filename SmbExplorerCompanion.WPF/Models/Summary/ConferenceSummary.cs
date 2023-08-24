using System.Collections.ObjectModel;

namespace SmbExplorerCompanion.WPF.Models.Summary;

public class ConferenceSummary
{
    public int Id { get; set; }
    public string ConferenceName { get; set; } = string.Empty;
    public ObservableCollection<DivisionSummary> Divisions { get; set; } = new();
}