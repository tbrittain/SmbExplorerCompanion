using System;
using System.Windows;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TeamOverviewViewModel : ViewModelBase
{
    public const string TeamIdProp = "TeamId";
    
    private readonly INavigationService _navigationService;

    public TeamOverviewViewModel(INavigationService navigationService)
    {
        _navigationService = navigationService;
        
        var ok = _navigationService.TryGetParameter<int>(TeamIdProp, out var teamId);
        if (!ok)
        {
            const string message = "Could not get team id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        _navigationService.ClearParameters();

        TeamId = teamId;
    }
    
    public int TeamId { get; set; }
}