using System;
using SmbExplorerCompanion.WPF.EventHandlers;

namespace SmbExplorerCompanion.WPF.Views;

public partial class PlayerOverviewView : IDisposable
{
    public PlayerOverviewView()
    {
        InitializeComponent();
        
        PlayerSeasonBattingDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerPlayoffBattingDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerSeasonPitchingDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerPlayoffPitchingDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
    }

    public void Dispose()
    {
        PlayerSeasonBattingDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerPlayoffBattingDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerSeasonPitchingDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayerPlayoffPitchingDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        GC.SuppressFinalize(this);
    }
}