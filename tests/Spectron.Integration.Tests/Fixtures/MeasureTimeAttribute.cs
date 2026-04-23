using System.Diagnostics;
using System.Reflection;
using Xunit.v3;

namespace OldBit.Spectron.Integration.Tests.Fixtures;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class MeasureTimeAttribute : BeforeAfterTestAttribute
{
    private const string StopwatchKey = "__measure_time_stopwatch";

    public override void Before(MethodInfo methodUnderTest, IXunitTest test) =>
        TestContext.Current.KeyValueStorage[StopwatchKey] = Stopwatch.StartNew();

    public override void After(MethodInfo methodUnderTest, IXunitTest test)
    {
        if (!TestContext.Current.KeyValueStorage.TryRemove(StopwatchKey, out var value) ||
            value is not Stopwatch stopwatch)
        {
            return;
        }

        stopwatch.Stop();

        TestContext.Current.TestOutputHelper?.WriteLine(
            "{0} took {1} ms",
            test.TestDisplayName,
            stopwatch.Elapsed.TotalMilliseconds);
    }
}