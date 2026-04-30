global using System;
global using System.Net;

global using DevStack.Application;
global using DevStack.Domain.Entities;
global using DevStack.Domain.Enums;
global using DevStack.Infrastructure;
global using DevStack.Persistence;

global using Microsoft.EntityFrameworkCore;
global using Microsoft.Extensions.Diagnostics.HealthChecks;
global using Microsoft.FeatureManagement;

global using OpenTelemetry.Metrics;
global using OpenTelemetry.Trace;

global using Serilog;
global using Serilog.Core;
global using Serilog.Events;
