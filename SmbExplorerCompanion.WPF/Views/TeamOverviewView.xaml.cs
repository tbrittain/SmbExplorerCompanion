using System;
using SmbExplorerCompanion.WPF.EventHandlers;

namespace SmbExplorerCompanion.WPF.Views;

public partial class TeamOverviewView : IDisposable
{
    public TeamOverviewView()
    {
        InitializeComponent();

        TopPlayersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        TeamSeasonsDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        ;
    }

    public void Dispose()
    {
        TopPlayersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        ;
        TeamSeasonsDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        ;
        GC.SuppressFinalize(this);
    }
}