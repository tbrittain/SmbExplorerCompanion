using System;
using SmbExplorerCompanion.WPF.EventHandlers;

namespace SmbExplorerCompanion.WPF.Views;

public partial class TeamSeasonDetailView : IDisposable
{
    public TeamSeasonDetailView()
    {
        InitializeComponent();
        
        SeasonBattersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        SeasonPitchersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayoffBattersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayoffPitchersDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
    }

    public void Dispose()
    {
        SeasonBattersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        SeasonPitchersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayoffBattersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        PlayoffPitchersDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        GC.SuppressFinalize(this);
    }
}