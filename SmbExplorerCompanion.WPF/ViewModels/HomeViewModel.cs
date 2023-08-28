using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Search;
using SmbExplorerCompanion.Core.Commands.Queries.Summary;
using SmbExplorerCompanion.Core.Entities.Search;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Search;
using SmbExplorerCompanion.WPF.Mappings.Summary;
using SmbExplorerCompanion.WPF.Models.Search;
using SmbExplorerCompanion.WPF.Models.Summary;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly IMediator _mediator;
    private string _searchQuery;
    private readonly INavigationService _navigationService;

    public HomeViewModel(IMediator mediator, INavigationService navigationService)
    {
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = Cursors.Wait);
        _mediator = mediator;
        _navigationService = navigationService;

        var franchiseSummaryResult = _mediator.Send(new GetFranchiseSummaryRequest()).Result;
        if (franchiseSummaryResult.TryPickT2(out var exception, out var rest))
        {
            MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }

        if (rest.TryPickT0(out var franchiseSummaryDto, out _))
        {
            var franchiseSummaryMapper = new FranchiseSummaryMapping();
            FranchiseSummary = franchiseSummaryMapper.FromFranchiseSummaryDto(franchiseSummaryDto);
        }
        
        var conferenceSummaryResult = _mediator.Send(new GetLeagueSummaryRequest()).Result;
        if (conferenceSummaryResult.TryPickT2(out exception, out var rest2))
        {
            MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
            return;
        }
        
        if (rest2.TryPickT0(out var leagueSummaryDto, out _))
        {
            var conferenceSummaryMapper = new ConferenceSummaryMapping();
            Conferences.AddRange(leagueSummaryDto.Select(conferenceSummaryMapper.FromConferenceSummaryDto));
        }
        
        Application.Current.Dispatcher.Invoke(() => Mouse.OverrideCursor = null);
    }

    public ObservableCollection<ConferenceSummary> Conferences { get; } = new();

    public ObservableGroupedCollection<SearchResultType, SearchResult> SearchResults { get; } = new();

    public FranchiseSummary? FranchiseSummary { get; }
    public Visibility FranchiseSummaryVisibility => FranchiseSummary?.NumPlayers > 0 ? Visibility.Visible : Visibility.Collapsed;

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

    private bool CanSearch() => !string.IsNullOrWhiteSpace(SearchQuery);

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task GetSearchResults()
    {
        HasSearched = true;
        SearchResults.Clear();
        OnPropertyChanged(nameof(NoSearchResultsVisibility));
        var searchResponse = await _mediator.Send(new GetSearchResultsQuery(SearchQuery.Trim()));
        if (searchResponse.TryPickT1(out var exception, out var searchResultDtos))
        {
            MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var searchResultMapper = new SearchResultMapping();
        var searchResults = searchResultDtos
            .Select(searchResultMapper.FromSearchResultDto)
            .ToList();

        var groupedSearchResults = searchResults
            .GroupBy(searchResult => searchResult.Type)
            .Select(group => new ObservableGroup<SearchResultType, SearchResult>(group.Key, group))
            .ToList();

        SearchResults.AddRange(groupedSearchResults);
        OnPropertyChanged(nameof(NoSearchResultsVisibility));
    }

    private bool HasSearched { get; set; }

    public Visibility NoSearchResultsVisibility => HasSearched && SearchResults.Count == 0 ? Visibility.Visible : Visibility.Collapsed;

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
}