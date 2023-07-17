using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Services;

public interface INavigationService
{
    ViewModelBase CurrentView { get; }
    void NavigateTo<T> () where T : ViewModelBase;
}