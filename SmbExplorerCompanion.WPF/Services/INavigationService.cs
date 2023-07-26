using System;
using System.ComponentModel;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Services;

public interface INavigationService : INotifyPropertyChanged
{
    ViewModelBase CurrentView { get; }
    void NavigateTo<T> () where T : ViewModelBase;
    void NavigateTo<T>(params Tuple<string, object>[] parameters) where T : ViewModelBase;
    void NavigateBack();
    bool CanNavigateBack { get; }
    bool TryGetParameter<T>(string parameterName, out T parameterValue) where T : notnull;
    void ClearParameters();
}