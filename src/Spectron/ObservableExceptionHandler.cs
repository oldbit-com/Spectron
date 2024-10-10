using System;
using System.Diagnostics;
using System.Reactive.Concurrency;
using ReactiveUI;

namespace OldBit.Spectron;

public class ObservableExceptionHandler : IObserver<Exception>
{
    public void OnCompleted()
    {
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }

        RxApp.MainThreadScheduler.Schedule(() => throw new NotImplementedException());
    }

    public void OnError(Exception error)
    {
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }

        Console.WriteLine(error.ToString());

        RxApp.MainThreadScheduler.Schedule(() => throw error);
    }

    public void OnNext(Exception error)
    {
        if (Debugger.IsAttached)
        {
            Debugger.Break();
        }

        Console.WriteLine(error.ToString());

        RxApp.MainThreadScheduler.Schedule(() => throw error) ;
    }
}