using System.Windows.Controls;

namespace SmbExplorerCompanion.WPF.EventHandlers;

public static class DataGridDefaultSortBehavior
{
    public static void DataGridOnSorting(object sender, DataGridSortingEventArgs e)
    {
        var dataGrid = (DataGrid) sender;
        dataGrid.SelectedItem = null;
    }
}