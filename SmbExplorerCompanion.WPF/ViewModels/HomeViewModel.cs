﻿using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Search;
using SmbExplorerCompanion.Core.Commands.Queries.Summary;
using SmbExplorerCompanion.Core.Entities.Search;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Search;
using SmbExplorerCompanion.WPF.Mappings.Summary;
using SmbExplorerCompanion.WPF.Models.Search;
using SmbExplorerCompanion.WPF.Models.Summary;
using SmbExplorerCompanion.WPF.Services;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMediator _mediator;
    private string _searchQuery;
    private readonly INavigationService _navigationService;

    public HomeViewModel(IApplicationContext applicationContext, IMediator mediator, INavigationService navigationService)
    {
        _applicationContext = applicationContext;
        _mediator = mediator;
        _navigationService = navigationService;

        var franchiseSummaryResult = _mediator.Send(new GetFranchiseSummaryRequest()).Result;
        if (franchiseSummaryResult.TryPickT2(out var exception, out var rest))
        {
            MessageBox.Show(exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        if (rest.TryPickT1(out _, out var franchiseSummaryDto))
        {
            return;
        }

        var franchiseSummaryMapper = new FranchiseSummaryMapping();
        FranchiseSummary = franchiseSummaryMapper.FromFranchiseSummaryDto(franchiseSummaryDto);
    }

    public ObservableGroupedCollection<SearchResultType, SearchResult> SearchResults { get; } = new();

    public FranchiseSummary FranchiseSummary { get; }
    public Visibility FranchiseSummaryVisibility => FranchiseSummary.NumPlayers > 0 ? Visibility.Visible : Visibility.Collapsed;

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
        if (FranchiseSummary.MostRecentChampionTeamId is null) return;
        
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
}