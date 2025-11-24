using System.Diagnostics;
using System.Diagnostics.Metrics;

namespace Neovortex.Mediator.OpenTelemetry;

public sealed class OpenTelemetryOptions
{
    public string InstrumentationName { get; set; } = "Mediator";
    public string? InstrumentationVersion { get; set; } = "1.0.0";

    public Func<Type, bool>? ShouldFilter { get; set; }
    public Action<Activity?, object, TagList>? EnrichActivity { get; set; }
    public bool RecordResponseData { get; set; } = false;
    public Action<Activity, object?>? EnrichActivityWithResponse { get; set; }

    public Func<ActivitySource>? ActivitySourceFactory { get; set; }
    public Func<Meter>? MeterFactory { get; set; }
    public Func<Meter, UpDownCounter<long>>? MessageActiveFactory { get; set; }
    public Func<Meter, Counter<long>>? MessageCountFactory { get; set; }
    public Func<Meter, Counter<long>>? MessageFailedFactory { get; set; }
    public Func<Meter, Histogram<double>>? MessageDurationFactory { get; set; }
}