using System;
using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;

namespace SmbExplorerCompanion.WPF.Views;

public partial class TeamOverviewView : UserControl, IDisposable
{
    public TeamOverviewView()
    {
        InitializeComponent();
        
        TopPlayersDataGrid.Sorting += DataGridOnSorting;
        TeamSeasonsDataGrid.Sorting += DataGridOnSorting;
    }

    private void DataGridOnSorting(object sender, DataGridSortingEventArgs e)
    {
        TopPlayersDataGrid.SelectedItem = null;
    }

    public void Dispose()
    {
        TopPlayersDataGrid.Sorting -= DataGridOnSorting;
        TeamSeasonsDataGrid.Sorting -= DataGridOnSorting;
        GC.SuppressFinalize(this);
    }
}