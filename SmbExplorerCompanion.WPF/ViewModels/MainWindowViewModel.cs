using System.Threading.Tasks;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    public MainWindowViewModel(INavigationService navigationService)
    {
        NavigationService = navigationService;
    }
    
    public INavigationService NavigationService { get; }
    
    public Task Initialize()
    {
        NavigationService.NavigateTo<LandingViewModel>();
        return Task.CompletedTask;
    }
}