using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private readonly IApplicationContext _applicationContext;

    public MainWindowViewModel(INavigationService navigationService, IApplicationContext applicationContext)
    {
        NavigationService = navigationService;
        _applicationContext = applicationContext;

        _applicationContext.PropertyChanged += ApplicationContextOnPropertyChanged;
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
        NavigationService.NavigateTo<LandingViewModel>();
        return Task.CompletedTask;
    }
    
    public bool SidebarEnabled => _applicationContext.IsFranchiseSelected;

    public Visibility SidebarVisibility =>
        _applicationContext.IsFranchiseSelected ? Visibility.Visible : Visibility.Collapsed;
}