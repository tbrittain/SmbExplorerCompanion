using System;
using System.Windows;
using System.Windows.Input;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public class TeamManagementViewModel : ViewModelBase
{
    public const string TeamIdProp = "TeamId";
    private readonly INavigationService _navigationService;

    public TeamManagementViewModel(INavigationService navigationService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _navigationService = navigationService;
        
        var ok = _navigationService.TryGetParameter<int>(TeamIdProp, out var teamId);
        if (!ok)
        {
            const string message = "Could not get team id from navigation parameters.";
            MessageBox.Show(message);
            throw new Exception(message);
        }

        TeamId = teamId;
    }

    private int TeamId { get; }
}