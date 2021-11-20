using System.Diagnostics;
using Xunit;
using Xunit2.Should;

namespace Infra.Net.Extensions.Tests.Helpers;

public class StopwatchTests
{

    [Fact]
    public void StopwatchMustReturnElapsedMillisContinuously()
    {
        var stopwatch = Stopwatch.StartNew();
        Thread.Sleep(1000);
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterOrEqualTo(1000);
        Thread.Sleep(500);
        stopwatch.ElapsedMilliseconds.ShouldBeGreaterOrEqualTo(1500);
    }

}