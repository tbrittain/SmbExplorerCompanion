using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Services;

public sealed class NavigationService : INavigationService
{
    private readonly Func<Type, ViewModelBase> _viewModelFactory;

    private ViewModelBase _currentViewModel = null!;
    private readonly Stack<ViewModelBase> _navigationStack = new();
    private readonly Dictionary<string, object> _parameters = new();

    public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    public ViewModelBase CurrentView
    {
        get => _currentViewModel;
        private set => SetField(ref _currentViewModel, value);
    }

    public void NavigateTo<TViewModelBase>() where TViewModelBase : ViewModelBase
    {
        var viewModelBase = _viewModelFactory.Invoke(typeof(TViewModelBase));
        _navigationStack.Push(viewModelBase);

        OnPropertyChanged(nameof(CanNavigateBack));
        CurrentView = viewModelBase;
    }

    public void NavigateTo<T>(params Tuple<string, object>[] parameters) where T : ViewModelBase
    {
        foreach (var (parameterName, parameterValue) in parameters)
        {
            _parameters.Add(parameterName, parameterValue);
        }
        
        var viewModelBase = _viewModelFactory.Invoke(typeof(T));
        _navigationStack.Push(viewModelBase);

        OnPropertyChanged(nameof(CanNavigateBack));
        CurrentView = viewModelBase;
    }

    public void NavigateBack()
    {
        if (!_navigationStack.Any()) return;

        _navigationStack.Pop();

        OnPropertyChanged(nameof(CanNavigateBack));
        CurrentView = _navigationStack.Peek();
    }

    public bool CanNavigateBack => _navigationStack.Any();

    public bool TryGetParameter<T>(string parameterName, out T parameterValue) where T : notnull
    {
        var parameterExists = _parameters.TryGetValue(parameterName, out var parameter);

        if (!parameterExists)
        {
            parameterValue = default!;
            return false;
        }

        parameterValue = (T) parameter!;
        return true;
    }

    public void ClearParameters()
    {
        _parameters.Clear();
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private void SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return;
        field = value;
        OnPropertyChanged(propertyName);
    }
}