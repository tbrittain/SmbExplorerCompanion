using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace SmbExplorerCompanion.WPF.Extensions;

public static class ObservableCollectionExtensions
{
    public static void AddRange<T>(this ObservableCollection<T> collection, IEnumerable<T> items)
    {
        foreach (var item in items)
        {
            collection.Add(item);
        }
    }

    public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> items)
    {
        return new(items);
    }
}