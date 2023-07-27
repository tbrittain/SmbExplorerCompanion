using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class HomeViewModel : ViewModelBase
{
    private readonly IApplicationContext _applicationContext;

    public HomeViewModel(IApplicationContext applicationContext)
    {
        _applicationContext = applicationContext;
    }
}