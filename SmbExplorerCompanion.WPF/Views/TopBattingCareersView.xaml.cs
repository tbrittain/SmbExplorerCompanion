using System.ComponentModel;
using System.Diagnostics;
using System.DirectoryServices;
using System.Windows.Controls;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Views;

public partial class TopBattingCareersView
{
    public TopBattingCareersView()
    {
        InitializeComponent();
    }

    private async void TopBattingCareersDataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        Debug.Assert(e.Column.SortMemberPath is not null, "e.Column.SortMemberPath is not null");
        var newSortColumn = e.Column.SortMemberPath;

        var viewModel = (TopBattingCareersViewModel) DataContext;
        TopBattingCareersDataGrid.Items.SortDescriptions.Clear();
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
        TopBattingCareersDataGrid.Items.SortDescriptions.Add(new SortDescription(newSortColumn, sortDirection));
        e.Column.SortDirection = sortDirection;

        await viewModel.GetTopBattingCareers();

        e.Handled = true;
    }
}