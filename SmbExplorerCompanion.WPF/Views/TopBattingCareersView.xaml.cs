using System;
using System.ComponentModel;
using System.Diagnostics;
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
        var columnPropertyName = e.Column.SortMemberPath;

        var viewModel = (TopBattingCareersViewModel) DataContext;
        viewModel.SortColumn = columnPropertyName;

        await viewModel.GetTopBattingCareers();
        
        TopBattingCareersDataGrid.Items.SortDescriptions.Clear();
        TopBattingCareersDataGrid.Items.SortDescriptions.Add(new SortDescription(columnPropertyName, ListSortDirection.Descending));
        
        e.Handled = true;
    }
}