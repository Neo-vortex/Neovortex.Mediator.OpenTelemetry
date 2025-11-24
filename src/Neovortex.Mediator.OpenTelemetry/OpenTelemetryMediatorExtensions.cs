using System.Diagnostics;
using System.Diagnostics.Metrics;
using Mediator;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Neovortex.Mediator.OpenTelemetry;


public static class OpenTelemetryMediatorExtensions
{
    public static IServiceCollection AddMediatorOpenTelemetry(
        this IServiceCollection services,
        Action<OpenTelemetryOptions>? configure = null)
    {
        var options = new OpenTelemetryOptions();
        configure?.Invoke(options);

        var activitySource = new ActivitySource(options.InstrumentationName, options.InstrumentationVersion);


        services.AddSingleton(options);
        services.TryAddSingleton(activitySource); 
        services.AddSingleton(typeof(IPipelineBehavior<,>), typeof(OpenTelemetryPipelineBehavior<,>));

        return services;
    }

    public static TracerProviderBuilder AddMediatorInstrumentation(this TracerProviderBuilder builder, string name  = "Mediator")
        => builder.AddSource(name);

}