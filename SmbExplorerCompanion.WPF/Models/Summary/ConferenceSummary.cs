using System.Collections.ObjectModel;
using System.Linq;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.WPF.Extensions;

namespace SmbExplorerCompanion.WPF.Models.Summary;

public class ConferenceSummary
{
    public int Id { get; set; }
    public string ConferenceName { get; set; } = string.Empty;
    public ObservableCollection<DivisionSummary> Divisions { get; set; } = new();
}

public static class ConferenceSummaryExtensions
{
    public static ConferenceSummary FromCore(this ConferenceSummaryDto x)
    {
        return new ConferenceSummary
        {
            Id = x.Id,
            ConferenceName = x.ConferenceName,
            Divisions = x.Divisions
                .Select(y => y.FromCore())
                .ToObservableCollection()
        };
    }
}