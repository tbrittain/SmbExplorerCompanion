using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
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

    private void NavigationServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // TODO: Was going to use this to conditionally change the background color of the selected sidebar item
        // switch (e.PropertyName)
        // {
        //     case nameof(INavigationService.CurrentView):
        //     {
        //         OnPropertyChanged(nameof(SidebarVisibility));
        //         break;
        //     }
        // }
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

    public INavigationService NavigationService { get; }

    public Task Initialize()
    {
        NavigationService.NavigateTo<FranchiseSelectViewModel>();
        return Task.CompletedTask;
    }
    
    public bool SidebarEnabled => _applicationContext.IsFranchiseSelected;
    
    [RelayCommand]
    public Task NavigateToHome()
    {
        NavigationService.NavigateTo<HomeViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    public Task NavigateToImportCsv()
    {
        NavigationService.NavigateTo<ImportCsvViewModel>();
        return Task.CompletedTask;
    }
}