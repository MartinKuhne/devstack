global using System.ComponentModel;
global using System.Text.Json;

global using DevStack.Application;
global using DevStack.Domain.Entities;
global using DevStack.Domain.Enums;
global using DevStack.Persistence;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.AI;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Logging;
global using Microsoft.FeatureManagement;

global using ModelContextProtocol.Server;

global using OpenTelemetry.Metrics;
global using OpenTelemetry.Trace;

global using Serilog;
global using Serilog.Core;
global using Serilog.Events;
