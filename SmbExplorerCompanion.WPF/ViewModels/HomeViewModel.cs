using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.Input;
using MediatR;
using SmbExplorerCompanion.Core.Commands.Queries.Search;
using SmbExplorerCompanion.Core.Entities.Search;
using SmbExplorerCompanion.Core.Interfaces;
using SmbExplorerCompanion.WPF.Extensions;
using SmbExplorerCompanion.WPF.Mappings.Search;
using SmbExplorerCompanion.WPF.Models.Search;

namespace SmbExplorerCompanion.WPF.ViewModels;

public partial class HomeViewModel : ViewModelBase
{
    private readonly IApplicationContext _applicationContext;
    private readonly IMediator _mediator;
    private string _searchQuery;

    public HomeViewModel(IApplicationContext applicationContext, IMediator mediator)
    {
        _applicationContext = applicationContext;
        _mediator = mediator;
    }

    public ObservableGroupedCollection<SearchResultType, SearchResult> SearchResults { get; } = new();

    public string SearchQuery
    {
        get => _searchQuery;
        set
        {
            SetField(ref _searchQuery, value);
            GetSearchResultsCommand.NotifyCanExecuteChanged();
        }
    }

    private bool CanSearch() => !string.IsNullOrWhiteSpace(SearchQuery);

    [RelayCommand(CanExecute = nameof(CanSearch))]
    private async Task GetSearchResults()
    {
        SearchResults.Clear();
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
    }
}