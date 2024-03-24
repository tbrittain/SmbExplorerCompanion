using System;
using SmbExplorerCompanion.WPF.EventHandlers;

namespace SmbExplorerCompanion.WPF.Views;

public partial class HistoricalTeamsView : IDisposable
{
    public HistoricalTeamsView()
    {
        InitializeComponent();
        TeamsDataGrid.Sorting += DataGridDefaultSortBehavior.DataGridOnSorting;
    }

    public void Dispose()
    {
        TeamsDataGrid.Sorting -= DataGridDefaultSortBehavior.DataGridOnSorting;
        GC.SuppressFinalize(this);
    }
}