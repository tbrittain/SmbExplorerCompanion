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

    private void TopSeasonPitchingDataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        Debug.Assert(e.Column.SortMemberPath is not null, "e.Column.SortMemberPath is not null");
        var columnPropertyName = e.Column.SortMemberPath;

        var viewModel = (TopPitchingSeasonsViewModel) DataContext;
        viewModel.SortColumn = columnPropertyName;

        viewModel.ShortCircuitPageNumberRefresh = true;
        viewModel.PageNumber = 1;
        viewModel.ShortCircuitPageNumberRefresh = false;
        viewModel.GetTopPitchingSeason();

        TopSeasonPitchingDataGrid.Items.SortDescriptions.Clear();
        TopSeasonPitchingDataGrid.Items.SortDescriptions.Add(new SortDescription(columnPropertyName, ListSortDirection.Descending));

        e.Handled = true;
    }
}