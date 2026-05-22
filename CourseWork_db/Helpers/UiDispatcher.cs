using System;
using System.Threading.Tasks;
using Avalonia.Threading;

namespace CourseWork_db.Helpers;

public static class UiDispatcher
{
    public static async Task RunAsync(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
            action();
        else
            await Dispatcher.UIThread.InvokeAsync(action);
    }
}
