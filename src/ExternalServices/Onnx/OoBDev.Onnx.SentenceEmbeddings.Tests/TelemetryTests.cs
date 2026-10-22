using Microsoft.VisualStudio.TestTools.UnitTesting;
using OoBDev.TestUtilities;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Threading.Tasks;

namespace OoBDev.Onnx.SentenceEmbeddings.Tests;

/// <summary>
/// Telemetry of the real model run; inconclusive when the model cannot be downloaded.
/// </summary>
[TestClass]
public class TelemetryTests
{
    [TestCategory(TestCategories.Integration)]
    [TestMethod]
    public async Task Generate_Batch_RecordsActivityAndMetrics()
    {
        // Stage
        using var generator = TestModel.CreateGenerator();
        var activities = new ConcurrentBag<Activity>();
        var measurements = new ConcurrentBag<(string Name, double Value)>();
        using var activityListener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == SentenceEmbeddingTelemetry.Name,
            Sample = (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activities.Add,
        };
        ActivitySource.AddActivityListener(activityListener);
        using var meterListener = new MeterListener
        {
            InstrumentPublished = (instrument, listener) =>
            {
                if (instrument.Meter.Name == SentenceEmbeddingTelemetry.Name) listener.EnableMeasurementEvents(instrument);
            },
        };
        meterListener.SetMeasurementEventCallback<double>((i, v, _, _) => measurements.Add((i.Name, v)));
        meterListener.SetMeasurementEventCallback<int>((i, v, _, _) => measurements.Add((i.Name, v)));
        meterListener.SetMeasurementEventCallback<long>((i, v, _, _) => measurements.Add((i.Name, v)));
        meterListener.Start();

        // Test
        await generator.GenerateAsync(["hello world", "another sentence"]);

        // Assert
        Assert.IsTrue(activities.Any(a => a.OperationName == "embeddings.batch"));
        // Other tests in the run share the static meter, so look for this batch rather than the only one
        Assert.IsTrue(measurements.Any(m => m.Name == "oobdev.embeddings.batch_size" && m.Value == 2d));
        Assert.IsTrue(measurements.Any(m => m.Name == "oobdev.embeddings.tokens" && m.Value > 0));
        Assert.IsTrue(measurements.Any(m => m.Name == "oobdev.embeddings.duration"));
    }
}
