using System;
using System.Reactive.Concurrency;
using ReactiveUI;

namespace OldBit.Spectron;

public class ObservableExceptionHandler : IObserver<Exception>
{
    public void OnCompleted()
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Break();
        }

        RxApp.MainThreadScheduler.Schedule(() => throw new NotImplementedException());
    }

    public void OnError(Exception error)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Break();
        }

        Console.WriteLine(error.ToString());

        RxApp.MainThreadScheduler.Schedule(() => throw error);
    }

    public void OnNext(Exception error)
    {
        if (System.Diagnostics.Debugger.IsAttached)
        {
            System.Diagnostics.Debugger.Break();
        }

        Console.WriteLine(error.ToString());

        RxApp.MainThreadScheduler.Schedule(() => throw error) ;
    }
}