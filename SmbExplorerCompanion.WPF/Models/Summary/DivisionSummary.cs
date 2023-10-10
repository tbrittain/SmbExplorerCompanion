using System.Collections.ObjectModel;

namespace SmbExplorerCompanion.WPF.Models.Summary;

public class DivisionSummary
{
    public int Id { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public ObservableCollection<TeamSummary> Teams { get; set; } = new();
}