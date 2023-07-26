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

    public NavigationService(Func<Type, ViewModelBase> viewModelFactory)
    {
        _viewModelFactory = viewModelFactory;
    }

    public Stack<ViewModelBase> NavigationStack { get; } = new();

    public ViewModelBase CurrentView
    {
        get => _currentViewModel;
        private set => SetField(ref _currentViewModel, value);
    }

    public void NavigateTo<TViewModelBase>() where TViewModelBase : ViewModelBase
    {
        var viewModelBase = _viewModelFactory.Invoke(typeof(TViewModelBase));
        NavigationStack.Push(viewModelBase);
        CurrentView = viewModelBase;
    }

    public void NavigateBack()
    {
        if (!NavigationStack.Any()) return;

        NavigationStack.Pop();
        CurrentView = NavigationStack.Peek();
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