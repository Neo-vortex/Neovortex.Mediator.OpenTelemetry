# Neovortex.Mediator.OpenTelemetry

**OpenTelemetry extension for Mediator** by Martinothamar 

This library provides an easy way to instrument [Mediator](https://github.com/martinothamar/Mediator) messages with  OpenTelemetry tracing in .NET applications. It automatically tracks messages and allows custom enrichment of telemetry data.

---

## Features

* Automatic tracing of Mediator requests.
* Filter messages to exclude certain types.
* Optionally record response data.
* Works with ASP.NET Core and any netstandard2

---

## Installation

You can install via NuGet:

```bash
dotnet add package Neovortex.Mediator.OpenTelemetry
```

---

## Getting Started

### Register the pipeline in your DI container

```csharp
using Neovortex.Mediator.OpenTelemetry;a

builder.Services.AddMediatorOpenTelemetry(options =>
{
    // Filter out certain message types
    options.ShouldFilter = type => type.Name.Contains("HealthCheck");

    // Add custom tags to activity
    options.EnrichActivity = (activity, message, tags) =>
    {
        tags.Add("custom.tag", "example");
    };

    // Record response data (optional)
    options.RecordResponseData = true;
    options.EnrichActivityWithResponse = (activity, response) =>
    {
        activity.SetTag("response.value", response?.ToString());
    };

});

builder.Services.AddOpenTelemetry()
    .ConfigureResource(r => r.AddService("MyAPI"))
    .WithTracing(t => t
        .SetSampler(new AlwaysOnSampler())
        .AddAspNetCoreInstrumentation()
        .AddMediatorInstrumentation()  // Adds source "Mediator"
        .AddConsoleExporter())
    .WithMetrics(m => m
        .AddAspNetCoreInstrumentation()
        .AddConsoleExporter());
```

---

## Example Usage in ASP.NET Core

```csharp
// Example: Sending a Mediator request
public class TestQuery(int testParam) : IRequest<string>
{
    public int TestParam { get; } = testParam;
}

public class TestQueryHandler : IRequestHandler<TestQuery, string>
{
    public async ValueTask<string> Handle(TestQuery request, CancellationToken cancellationToken)
    {
        await Task.Delay(100, cancellationToken);
        return ((request.TestParam * 2).ToString());
    }
}

// Somewhere in your controller or service
var result = await mediator.Send(new Ping());
```

Here is what you should see

```
Activity.TraceId:            936c2ef35ac1dda276c68f15f958d17f
Activity.SpanId:             89d9d3be42f883f7
Activity.TraceFlags:         Recorded
Activity.ParentSpanId:       f177be942bd46a02
Activity.DisplayName:        TestQuery
Activity.Kind:               Internal
Activity.StartTime:          2025-11-24T13:29:26.2257521Z
Activity.Duration:           00:00:00.1034930
Activity.Tags:
    messaging.system: mediator
    messaging.operation: process
    messaging.time.duration: 102
    messaging.operation.status: success
    result: 20
StatusCode: Ok
Instrumentation scope (ActivitySource):
    Name: Microsoft.AspNetCore
Resource associated with Activity:
    service.name: MyCompanyAPI
    service.instance.id: 76b25550-2214-4f09-b484-a13b1567829e
    telemetry.sdk.name: opentelemetry
    telemetry.sdk.language: dotnet
    telemetry.sdk.version: 1.14.0
```

---

## Configuration Options

| Option                       | Type                                 | Description                                         |
| ---------------------------- | ------------------------------------ | --------------------------------------------------- |
| `ShouldFilter`               | `Func<Type, bool>`                   | Exclude certain message types from instrumentation. |
| `EnrichActivity`             | `Action<Activity?, object, TagList>` | Add custom tags to activities.                      |
| `RecordResponseData`         | `bool`                               | Record response data in activities.                 |
| `EnrichActivityWithResponse` | `Action<Activity, object?>`          | Enrich activities with response details.            |


---

## Use Cases
* Integrate with observability platforms (e.g., Jaeger, Prometheus, OpenTelemetry Collector).
* Easily customize instrumentation per project needs.

---

## License

MIT License
