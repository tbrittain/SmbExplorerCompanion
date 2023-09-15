using System.ComponentModel;

namespace SmbExplorerCompanion.Core.Interfaces;

public interface IApplicationContext
{
    public int? SelectedFranchiseId { get; set; }
    public bool IsFranchiseSelected { get; }
    public bool HasFranchiseData { get; set; }
    event PropertyChangedEventHandler? PropertyChanged;
}