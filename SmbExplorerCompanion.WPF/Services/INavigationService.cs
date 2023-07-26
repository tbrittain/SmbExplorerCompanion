using System.Collections.Generic;
using System.ComponentModel;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Services;

public interface INavigationService : INotifyPropertyChanged
{
    Stack<ViewModelBase> NavigationStack { get; }
    ViewModelBase CurrentView { get; }
    void NavigateTo<T> () where T : ViewModelBase;
    void NavigateBack();
}