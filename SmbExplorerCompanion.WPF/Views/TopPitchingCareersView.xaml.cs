using System.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Views;

public partial class TopPitchingCareersView : UserControl
{
    public TopPitchingCareersView()
    {
        InitializeComponent();
    }

    private async void TopPitchingCareersDataGrid_OnSorting(object sender, DataGridSortingEventArgs e)
    {
        Debug.Assert(e.Column.SortMemberPath is not null, "e.Column.SortMemberPath is not null");
        var columnPropertyName = e.Column.SortMemberPath;

        var viewModel = (TopPitchingCareersViewModel) DataContext;
        viewModel.SortColumn = columnPropertyName;

        await viewModel.GetTopPitchingCareers();

        TopPitchingCareersDataGrid.Items.SortDescriptions.Clear();
        TopPitchingCareersDataGrid.Items.SortDescriptions.Add(new SortDescription(columnPropertyName, ListSortDirection.Descending));

        e.Handled = true;
    }
}