using System.Diagnostics;
using System.Diagnostics.Metrics;
using Mediator;
using OpenTelemetry.Trace;

namespace Neovortex.Mediator.OpenTelemetry;

public sealed class OpenTelemetryPipelineBehavior<TMessage, TResponse>(
    OpenTelemetryOptions options,
    ActivitySource activitySource)
    : IPipelineBehavior<TMessage, TResponse>
    where TMessage : IMessage
{
     public async ValueTask<TResponse> Handle(
        TMessage message,
        MessageHandlerDelegate<TMessage, TResponse> next,
        CancellationToken cancellationToken)
    {
        var messageType = typeof(TMessage);
        var messageName = messageType.Name;

        // Early filter
        if (options.ShouldFilter?.Invoke(messageType) == true)
            return await next(message, cancellationToken);

        // Start trace
        using var activity = activitySource.StartActivity(messageName, ActivityKind.Internal);
        activity?.AddTag("messaging.system", "mediator");
        activity?.AddTag("messaging.operation", "process");

        var tags = new TagList
        {
            { "messaging.system", "mediator" },
            { "messaging.operation", "process" },
            { "messaging.message.type", messageName },
            { "code.namespace", messageType.Namespace ?? "global" }
        };

        options.EnrichActivity?.Invoke(activity, message, tags);

        var sw = Stopwatch.StartNew();

        try
        {
            var response = await next(message, cancellationToken).ConfigureAwait(false);
            sw.Stop();
            activity?.AddTag("messaging.time.duration", sw.ElapsedMilliseconds.ToString());

            activity?.SetStatus(ActivityStatusCode.Ok);
            activity?.AddTag("messaging.operation.status", "success");

            if (options.RecordResponseData && activity is not null)
                options.EnrichActivityWithResponse?.Invoke(activity, response);
            return response;
        }
        catch (Exception ex)
        {
            sw.Stop();

            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            activity?.AddException(ex);
            activity?.AddTag("error", true);
            tags.Add("messaging.operation.status", "error");
            throw;
        }
    }
    
}