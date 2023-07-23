using System.ComponentModel;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Services;

public interface INavigationService : INotifyPropertyChanged
{
    ViewModelBase CurrentView { get; }
    void NavigateTo<T> () where T : ViewModelBase;
}