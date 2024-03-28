using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Views;

public partial class TopBattingSeasonsView
{
    public TopBattingSeasonsView()
    {
        InitializeComponent();
    }

    private async void TopSeasonBattingDataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        Debug.Assert(e.Column.SortMemberPath is not null, "e.Column.SortMemberPath is not null");
        var newSortColumn = e.Column.SortMemberPath;

        var viewModel = (TopBattingSeasonsViewModel) DataContext;

        viewModel.ShortCircuitPageNumberRefresh = true;
        viewModel.PageNumber = 1;
        viewModel.ShortCircuitPageNumberRefresh = false;
        
        TopSeasonBattingDataGrid.Items.SortDescriptions.Clear();
        ListSortDirection sortDirection;
        if (viewModel.SortColumn == newSortColumn)
        {
            viewModel.SortDescending = !viewModel.SortDescending;
            sortDirection = viewModel.SortDescending ? ListSortDirection.Descending : ListSortDirection.Ascending;
        }
        else
        {
            viewModel.SortDescending = true;
            sortDirection = ListSortDirection.Descending;
        }

        viewModel.SortColumn = newSortColumn;
        TopSeasonBattingDataGrid.Items.SortDescriptions.Add(new SortDescription(newSortColumn, sortDirection));
        e.Column.SortDirection = sortDirection;
        
        await viewModel.GetTopBattingSeason();

        e.Handled = true;
    }
}