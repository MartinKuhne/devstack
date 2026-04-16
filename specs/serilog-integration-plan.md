# Serilog Integration Plan

## Overview
Enhance the DevStack API with comprehensive Serilog logging including ASP.NET Core enrichers and HotChocolate error logging.

## Current State
- Serilog.AspNetCore and Serilog.Sinks.Console already installed
- Basic Serilog configuration in Program.cs with:
  - Configuration from appsettings
  - Service provider integration
  - Application and Environment properties
  - FromLogContext enrichment
  - Custom SensitiveDataDestructuringPolicy
- No HTTP request logging middleware
- No HotChocolate error logging
- Limited logging in handlers (only MCP endpoints have logging)

## Implementation Tasks

### 1. Add Serilog Enrichers (Task #152)
**Packages to add:**
- `Serilog.Enrichers.AspNetCore` - HTTP request context enrichment
- `Serilog.Enrichers.Environment` - Machine name, process ID, thread ID

**Changes:**
- Update `DevStack.Api.csproj` to add packages
- Update `Program.cs` to use enrichers

### 2. Configure HotChocolate Error Logging (Task #153)
**Implementation:**
- Create `GraphQLExecutionEventListener` class inheriting from `ExecutionDiagnosticEventListener`
- Inject `ILogger<GraphQLExecutionEventListener>`
- Override `RequestError` method to log GraphQL errors
- Register with `builder.Services.AddGraphQLServer()` using `.ConfigureExecutionOptions()`

**Reference implementation:**
```csharp
public class GraphQLExecutionEventListener : ExecutionDiagnosticEventListener
{
    private readonly ILogger<GraphQLExecutionEventListener> _logger;

    public GraphQLExecutionEventListener(ILogger<GraphQLExecutionEventListener> logger)
        => _logger = logger;

    public override void RequestError(IRequestContext context, Exception exception)
    {
        _logger.LogError(exception, "GraphQL request error: {Message}", exception.Message);
    }
}
```

### 3. Add Serilog Request Logging Middleware (Task #154)
**Configuration:**
- Add `UseSerilogRequestLogging()` before routing/middleware
- Customize with:
  - Message template
  - GetLevel callback for conditional logging levels
  - EnrichDiagnosticContext for additional HTTP properties

**Implementation:**
```csharp
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.GetLevel = (httpContext, elapsed, ex) =>
    {
        if (ex != null) return LogEventLevel.Error;
        if (httpContext.Response.StatusCode > 400 && httpContext.Response.StatusCode < 500) return LogEventLevel.Warning;
        if (httpContext.Response.StatusCode > 500) return LogEventLevel.Error;
        return LogEventLevel.Information;
    };
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("UserAgent", httpContext.Request.Headers["User-Agent"].FirstOrDefault());
        diagnosticContext.Set("ContentType", httpContext.Request.ContentType);
    };
});
```

### 4. Enhance Logging Throughout Codebase (Task #155)
**Areas to inspect:**
- `DevStack.Infrastructure` handlers (CreateProjectHandler, UpdateProjectHandler, etc.)
- `DevStack.Application` services
- `DevStack.Domain` services
- ErrorHandlingMiddleware (already has logging, but could be enhanced)

**Logging patterns to add:**
- Information logs for successful operations
- Warning logs for expected error conditions
- Error logs for exceptions
- Debug logs for detailed operation tracing (development only)

## Quality Gates
- [ ] Code compiles with no warnings
- [ ] All tests pass
- [ ] Logging configuration works in Development and Production
- [ ] GraphQL errors are properly logged
- [ ] HTTP requests are logged with enrichment
