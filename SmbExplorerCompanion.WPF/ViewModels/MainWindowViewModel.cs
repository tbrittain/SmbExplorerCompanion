using System;
using System.ComponentModel;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Input;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.Core.ValueObjects;
using SmbExplorerCompanion.WPF.Services;
using SmbExplorerCompanion.WPF.Utils;
using static SmbExplorerCompanion.Shared.Constants.Github;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private readonly IApplicationContext _applicationContext;
    private readonly IHttpService _httpService;
    private AppUpdateResult? _appUpdateResult;
    private bool _isUpdateAvailable;

    public MainWindowViewModel(INavigationService navigationService, IApplicationContext applicationContext, IHttpService httpService)
    {
        NavigationService = navigationService;
        _applicationContext = applicationContext;
        _httpService = httpService;

        _applicationContext.PropertyChanged += ApplicationContextOnPropertyChanged;
        NavigationService.PropertyChanged += NavigationServiceOnPropertyChanged;
    }

    public INavigationService NavigationService { get; }

    public bool SidebarEnabled => _applicationContext.IsFranchiseSelected;

    private void NavigationServiceOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
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
        _ = Task.Run(async () => await CheckForUpdates());
        return Task.CompletedTask;
    }

    private AppUpdateResult? AppUpdateResult
    {
        get => _appUpdateResult;
        set
        {
            SetField(ref _appUpdateResult, value);
            IsUpdateAvailable = true;
        }
    }

    public string UpdateAvailableDisplayText => IsUpdateAvailable
        ? $"Update Available: {AppUpdateResult?.Version.ToString()}"
        : "No Updates Available";

    public bool IsUpdateAvailable
    {
        get => _isUpdateAvailable;
        set
        {
            SetField(ref _isUpdateAvailable, value);
            OnPropertyChanged(nameof(UpdateAvailableDisplayText));
        }
    }

    private async Task CheckForUpdates()
    {
        var updateResult = await _httpService.CheckForUpdates();

        if (updateResult.TryPickT2(out var error, out var rest))
        {
            MessageBox.Show($"Failed to check for updates: {error.Value}",
                "Update Check Failed",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        if (rest.TryPickT1(out _, out var appUpdateResult))
        {
            // No update available
            return;
        }

        AppUpdateResult = appUpdateResult;

        if (appUpdateResult.Version.Major <= CurrentVersion.Major &&
            appUpdateResult.Version.Minor <= CurrentVersion.Minor) return;

        var message = $"An update is available ({CurrentVersion} --> {appUpdateResult.Version}, released " +
                      $"{appUpdateResult.DaysSinceRelease} days ago). Would you like open the release page?";

        var messageBoxResult = MessageBox.Show(message,
            "Update Available",
            MessageBoxButton.YesNo,
            MessageBoxImage.Information);

        if (messageBoxResult != MessageBoxResult.Yes) return;

        SafeProcess.Start(appUpdateResult.ReleasePageUrl);
    }
    
    public static string CurrentVersionString => $"Version {CurrentVersion}";

    private static Version CurrentVersion
    {
        get
        {
            var currentVersion = Assembly.GetEntryAssembly()?.GetName().Version;
            return currentVersion is null
                ? new Version(0, 0, 0, 0)
                : new Version(currentVersion.Major, currentVersion.Minor, currentVersion.Build);
        }
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

    [RelayCommand]
    private Task NavigateToSeasonPitching()
    {
        NavigationService.NavigateTo<TopPitchingSeasonsViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task NavigateToDelegateAwards()
    {
        NavigationService.NavigateTo<DelegateAwardsViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private Task NavigateToDelegateHallOfFamers()
    {
        NavigationService.NavigateTo<DelegateHallOfFamersViewModel>();
        return Task.CompletedTask;
    }

    [RelayCommand]
    private void OpenWiki()
    {
        SafeProcess.Start(WikiUrl);
    }

    [RelayCommand]
    private void SubmitFeatureRequest()
    {
        SafeProcess.Start(FeatureRequestUrl);
    }

    [RelayCommand]
    private void OpenDiscussions()
    {
        SafeProcess.Start(DiscussionsUrl);
    }

    [RelayCommand]
    private void OpenIssues()
    {
        SafeProcess.Start(IssuesUrl);
    }

    [RelayCommand]
    private void SubmitBugReport()
    {
        SafeProcess.Start(NewBugUrl);
    }

    [RelayCommand]
    private void OpenGithubRepo()
    {
        SafeProcess.Start(RepoUrl);
    }
    
    [RelayCommand]
    private void OpenUpdateVersionReleasePage()
    {
        SafeProcess.Start(AppUpdateResult!.Value.ReleasePageUrl);
    }

    override public void Dispose()
    {
        _applicationContext.PropertyChanged -= ApplicationContextOnPropertyChanged;
        NavigationService.PropertyChanged -= NavigationServiceOnPropertyChanged;
        GC.SuppressFinalize(this);
    }
}