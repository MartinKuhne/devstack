using System.Diagnostics;

using Serilog;
using Serilog.Configuration;
using Serilog.Core;
using Serilog.Events;

namespace DevStack.Mcp.Logging;

public class CorrelationIdEnricher : ILogEventEnricher
{
    public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
    {
        var traceId = Activity.Current?.TraceId.ToString() ?? "none";
        logEvent.AddPropertyIfAbsent(propertyFactory.CreateProperty("CorrelationId", traceId));
    }
}

public static class CorrelationIdEnricherExtensions
{
    public static LoggerConfiguration WithCorrelationId(
        this LoggerEnrichmentConfiguration enrichmentConfiguration)
    {
        if (enrichmentConfiguration is null)
            throw new ArgumentNullException(nameof(enrichmentConfiguration));

        return enrichmentConfiguration.With(new CorrelationIdEnricher());
    }
}
