using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Players;
using SmbExplorerCompanion.Core.Commands.Queries.Search;
using SmbExplorerCompanion.Core.Commands.Queries.Summary;
using SmbExplorerCompanion.Core.Entities.Search;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Models.Search;
using SmbExplorerCompanion.WPF.Models.Summary;
using SmbExplorerCompanion.WPF.Services;
using SmbExplorerCompanion.WPF.Utils;
using static SmbExplorerCompanion.Shared.Constants.Github;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly IApplicationContext _applicationContext;
    private readonly MappingService _mappingService;
    private readonly IMediator _mediator;
    private readonly INavigationService _navigationService;
    private bool _canDisplayFranchiseSummary;
    private FranchiseSummary? _franchiseSummary;
    private string _searchQuery = string.Empty;

    public HomeViewModel(IMediator mediator,
        INavigationService navigationService,
        IApplicationContext applicationContext,
        MappingService mappingService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _mediator = mediator;
        _navigationService = navigationService;
        _applicationContext = applicationContext;
        _mappingService = mappingService;

        if (applicationContext.HasFranchiseData)
        {
            CanDisplayFranchiseSummary = true;
            GetFranchiseSummary().Wait();
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        // Will only display a home page with information about getting started rather than
        // summary data about the franchise
        CanDisplayFranchiseSummary = false;
        _applicationContext.PropertyChanged += ApplicationContextOnPropertyChanged;
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public ObservableCollection<ConferenceSummary> Conferences { get; } = new();

    public ObservableGroupedCollection<SearchResultType, SearchResult> SearchResults { get; } = new();

    public FranchiseSummary? FranchiseSummary
    {
        get => _franchiseSummary;
        private set => SetField(ref _franchiseSummary, value);
    }

    public bool CanDisplayFranchiseSummary
    {
        get => _canDisplayFranchiseSummary;
        set => SetField(ref _canDisplayFranchiseSummary, value);
    }

    public int SearchRow => FranchiseSummary is null ? 1 : 2;

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            SetField(ref _searchQuery, value);
            if (SearchResults.Count > 0)
            {
                HasSearched = false;
                SearchResults.Clear();
            }

            GetSearchResultsCommand.NotifyCanExecuteChanged();
        }
    }

    private bool HasSearched { get; set; }

    public bool HasSearchResults => HasSearched && SearchResults.Count > 0;

    private async void ApplicationContextOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(_applicationContext.HasFranchiseData):
            {
                await Application.Current.Dispatcher.InvokeAsync(async () =>
                {
                    CanDisplayFranchiseSummary = _applicationContext.HasFranchiseData;
                    if (CanDisplayFranchiseSummary) await GetFranchiseSummary();
                });
                break;
            }
        }
    }

    private async Task GetFranchiseSummary()
    {
        var franchiseSummaryDto = await _mediator.Send(new GetFranchiseSummaryRequest());

        if (franchiseSummaryDto is not null)
            FranchiseSummary = await _mappingService.FromCore(franchiseSummaryDto);

        var leagueSummaryDto = await _mediator.Send(new GetLeagueSummaryRequest());
        Conferences.AddRange(leagueSummaryDto.Select(x => x.FromCore()));
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    private bool CanSearch()
    {
        return !string.IsNullOrWhiteSpace(SearchQuery);
    }

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task GetSearchResults()
    {
        HasSearched = true;
        SearchResults.Clear();
        OnPropertyChanged(nameof(HasSearchResults));
        var searchResultDtos = await _mediator.Send(new GetSearchResultsQuery(SearchQuery.Trim()));

        var groupedSearchResults = searchResultDtos
            .Select(x => x.FromCore())
            .GroupBy(searchResult => searchResult.Type)
            .Select(group => new ObservableGroup<SearchResultType, SearchResult>(group.Key, group))
            .ToList();

        SearchResults.AddRange(groupedSearchResults);
        OnPropertyChanged(nameof(HasSearchResults));
    }

    [RelayCommand]
    private void NavigateToSearchResultPage(SearchResult searchResult)
    {
        switch (searchResult.Type)
        {
            case SearchResultType.Players:
            {
                NavigateToPlayerOverviewPage(searchResult.Id);
                break;
            }
            case SearchResultType.Teams:
                NavigateToTeamOverviewPage(searchResult.Id);
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    [RelayCommand]
    private void NavigateToChampionPage()
    {
        if (FranchiseSummary?.MostRecentChampionTeamId is null) return;

        NavigateToTeamOverviewPage(FranchiseSummary.MostRecentChampionTeamId.Value);
    }

    [RelayCommand]
    private void NavigateToPlayerOverviewPage(int playerId)
    {
        var playerParams = new Tuple<string, object>[]
        {
            new(PlayerOverviewViewModel.PlayerIdProp, playerId)
        };
        _navigationService.NavigateTo<PlayerOverviewViewModel>(playerParams);
    }

    [RelayCommand]
    private async Task NavigateToRandomPlayerOverviewPage()
    {
        var player = await _mediator.Send(new GetRandomPlayerRequest());
        NavigateToPlayerOverviewPage(player.PlayerId);
    }

    private void NavigateToTeamOverviewPage(int teamId)
    {
        var teamParams = new Tuple<string, object>[]
        {
            new(TeamOverviewViewModel.TeamIdProp, teamId)
        };
        _navigationService.NavigateTo<TeamOverviewViewModel>(teamParams);
    }

    [RelayCommand]
    private void NavigateToTeamSeasonDetailPage(int seasonTeamId)
    {
        var teamSeasonParams = new Tuple<string, object>[]
        {
            new(TeamSeasonDetailViewModel.SeasonTeamIdProp, seasonTeamId)
        };
        _navigationService.NavigateTo<TeamSeasonDetailViewModel>(teamSeasonParams);
    }

    [RelayCommand]
    private void NavigateToGettingStartedWikiPage()
    {
        var url = Path.Combine(WikiUrl, "Getting-Started");
        SafeProcess.Start(url);
    }

    [RelayCommand]
    private void NavigateToSmbExplorerReleaseUrl()
    {
        SafeProcess.Start(SmbExplorerLatestReleaseUrl);
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _applicationContext.PropertyChanged -= ApplicationContextOnPropertyChanged;

        base.Dispose(disposing);
    }
}