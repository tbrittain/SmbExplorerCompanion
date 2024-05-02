using System.Collections.ObjectModel;
using System.Linq;
using SmbExplorerCompanion.Core.Entities.Summary;
using SmbExplorerCompanion.WPF.Extensions;

namespace SmbExplorerCompanion.WPF.Models.Summary;

public class DivisionSummary
{
    public int Id { get; set; }
    public string DivisionName { get; set; } = string.Empty;
    public ObservableCollection<TeamSummary> Teams { get; set; } = new();
}

public static class DivisionSummaryExtensions
{
    public static DivisionSummary FromCore(this DivisionSummaryDto x)
    {
        return new DivisionSummary
        {
            Id = x.Id,
            DivisionName = x.DivisionName,
            Teams = x.Teams
                .Select(y => y.FromCore())
                .ToObservableCollection()
        };
    }
}