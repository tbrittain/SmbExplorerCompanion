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
        var columnPropertyName = e.Column.SortMemberPath;

        var viewModel = (TopBattingSeasonsViewModel) DataContext;
        viewModel.SortColumn = columnPropertyName;

        await viewModel.GetTopBattingSeason();

        TopSeasonBattingDataGrid.Items.SortDescriptions.Clear();
        TopSeasonBattingDataGrid.Items.SortDescriptions.Add(new SortDescription(columnPropertyName, ListSortDirection.Descending));

        e.Handled = true;
    }
}