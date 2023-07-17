using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SmbExplorerCompanion.WPF.Services;

public interface IApplicationContext
{
    public int? SelectedFranchiseId { get; set; }
    public bool IsFranchiseSelected { get; }
    event PropertyChangedEventHandler? PropertyChanged;
}

public sealed class ApplicationContext : IApplicationContext, INotifyPropertyChanged
{
    private int? _selectedFranchiseId;

    public int? SelectedFranchiseId
    {
        get => _selectedFranchiseId;
        set
        {
            SetField(ref _selectedFranchiseId, value);
            OnPropertyChanged(nameof(IsFranchiseSelected));
        }
    }

    public bool IsFranchiseSelected => SelectedFranchiseId is not null;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}