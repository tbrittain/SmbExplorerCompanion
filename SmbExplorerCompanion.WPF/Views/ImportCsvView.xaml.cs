using System;
using System.Collections.Specialized;
using System.Windows;
using SmbExplorerCompanion.WPF.ViewModels;

namespace SmbExplorerCompanion.WPF.Views;

public partial class ImportCsvView : IDisposable
{
    public ImportCsvView()
    {
        InitializeComponent();

        Loaded += OnLoaded;
    }

    public void Dispose()
    {
        var dataContext = (ImportCsvViewModel) DataContext;
        dataContext.ImportProgress.CollectionChanged -= ImportProgressOnCollectionChanged;
        GC.SuppressFinalize(this);
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        var dataContext = (ImportCsvViewModel) DataContext;
        dataContext.ImportProgress.CollectionChanged += ImportProgressOnCollectionChanged;
    }

    private void ImportProgressOnCollectionChanged(object? _, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is not NotifyCollectionChangedAction.Add) return;
        LogScrollViewer.ScrollToEnd();
    }
}