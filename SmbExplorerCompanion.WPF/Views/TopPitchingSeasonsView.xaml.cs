using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Views;

public partial class TopPitchingSeasonsView
{
    public TopPitchingSeasonsView()
    {
        InitializeComponent();
    }

    private async void TopSeasonPitchingDataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        Debug.Assert(e.Column.SortMemberPath is not null, "e.Column.SortMemberPath is not null");
        var newSortColumn = e.Column.SortMemberPath;

        var viewModel = (TopPitchingSeasonsViewModel) DataContext;

        viewModel.ShortCircuitPageNumberRefresh = true;
        viewModel.PageNumber = 1;
        viewModel.ShortCircuitPageNumberRefresh = false;

        TopSeasonPitchingDataGrid.Items.SortDescriptions.Clear();
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
        TopSeasonPitchingDataGrid.Items.SortDescriptions.Add(new SortDescription(newSortColumn, sortDirection));
        e.Column.SortDirection = sortDirection;

        await viewModel.GetTopPitchingSeason();

        e.Handled = true;
    }
}