using System;
using System.Windows.Threading;

namespace SmbExplorerCompanion.WPF.Helpers;

public class Debouncer
{
    private readonly DispatcherTimer _timer;
    private Action _action;

    public Debouncer(Action action, TimeSpan delay = default)
    {
        _action = action;
        if (delay == default) delay = TimeSpan.FromMilliseconds(500);
        
        _timer = new DispatcherTimer { Interval = delay };
        _timer.Tick += TimerOnTick;
    }

    private void TimerOnTick(object sender, EventArgs eventArgs)
    {
        _timer.Stop();
        _action?.Invoke();
    }

    public void Debounce(Action actionToDebounce)
    {
        _action = actionToDebounce;
        _timer.Stop();
        _timer.Start();
    }
}