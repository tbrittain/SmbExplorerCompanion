using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.Input;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IApplicationContext _applicationContext;

    public MainWindowViewModel(INavigationService navigationService, IApplicationContext applicationContext)
    {
        NavigationService = navigationService;
        _applicationContext = applicationContext;

        _applicationContext.PropertyChanged += ApplicationContextOnPropertyChanged;
        NavigationService.PropertyChanged += NavigationServiceOnPropertyChanged;
    }

    public INavigationService NavigationService { get; }

    public bool SidebarEnabled => _applicationContext.IsFranchiseSelected;

    private void NavigationServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // TODO: Was going to use this to conditionally change the background color of the selected sidebar item
        switch (e.PropertyName)
        {
            case nameof(INavigationService.CanNavigateBack):
            {
                NavigateBackCommand.NotifyCanExecuteChanged();
                break;
            }
        }
    }

    private bool CanNavigateBack()
    {
        return NavigationService.CanNavigateBack;
    }

    [RelayCommand(CanExecute = nameof(CanNavigateBack))]
    private void NavigateBack()
    {
        NavigationService.NavigateBack();
    }

    private void ApplicationContextOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IApplicationContext.IsFranchiseSelected):
            {
                OnPropertyChanged(nameof(SidebarEnabled));
                break;
            }
        }
    }

    public Task Initialize()
    {
        NavigationService.NavigateTo<FranchiseSelectViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task NavigateToHome()
    {
        NavigationService.NavigateTo<HomeViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task NavigateToImportCsv()
    {
        NavigationService.NavigateTo<ImportCsvViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task NavigateToTeams()
    {
        NavigationService.NavigateTo<HistoricalTeamsViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task NavigateToCareerBatting()
    {
        NavigationService.NavigateTo<TopBattingCareersViewModel>();
        return Task.CompletedTask;
    }
    
    [RelayCommand]
    private Task NavigateToCareerPitching()
    {
        NavigationService.NavigateTo<TopPitchingCareersViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task NavigateToSeasonBatting()
    {
        NavigationService.NavigateTo<TopBattingSeasonsViewModel>();
        return Task.CompletedTask;
    }

    override public void Dispose()
    {
        _applicationContext.PropertyChanged -= ApplicationContextOnPropertyChanged;
        NavigationService.PropertyChanged -= NavigationServiceOnPropertyChanged;
        base.Dispose();
    }
}