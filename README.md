# Neovortex.Mediator.OpenTelemetry

**OpenTelemetry extension for Mediator** by Martinothamar 

This library provides an easy way to instrument [Mediator](https://github.com/martinothamar/Mediator) messages with  OpenTelemetry tracing in .NET applications. It automatically tracks messages and allows custom enrichment of telemetry data.

---

## Features

* Automatic tracing of Mediator requests.
* Filter messages to exclude certain types.
* Optionally record response data.
* Works with ASP.NET Core and any .NET 10+ project.

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
public class Ping : IRequest<string> {}

public class PingHandler : IRequestHandler<Ping, string>
{
    public Task<string> Handle(Ping request, CancellationToken cancellationToken)
    {
        return Task.FromResult("Pong");
    }
}

// Somewhere in your controller or service
var result = await mediator.Send(new Ping());
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
