using System.Windows;
using System.Windows.Controls;
using SmbExplorerCompanion.WPF.Models.Players;

namespace SmbExplorerCompanion.WPF.DataTemplateSelectors;

public class AwardTemplateSelector : DataTemplateSelector
{
    public DataTemplate FullWidthTemplate { get; set; } = null!;
    public DataTemplate HalfWidthTemplate { get; set; } = null!;
    
    override public DataTemplate SelectTemplate(object? item, DependencyObject container)
    {
        if (item is FormattedPlayerAward award)
        {
            return award.FullWidth ? FullWidthTemplate : HalfWidthTemplate;
        }

        return base.SelectTemplate(item, container);
    }
}