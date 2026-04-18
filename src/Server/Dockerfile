# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DevStack.slnx", "."]
COPY ["DevStack.Api/DevStack.Api.csproj", "DevStack.Api/"]
COPY ["DevStack.Application/DevStack.Application.csproj", "DevStack.Application/"]
COPY ["DevStack.Domain/DevStack.Domain.csproj", "DevStack.Domain/"]
COPY ["DevStack.Infrastructure/DevStack.Infrastructure.csproj", "DevStack.Infrastructure/"]
COPY ["DevStack.Persistence/DevStack.Persistence.csproj", "DevStack.Persistence/"]
COPY ["DevStack.Contracts/DevStack.Contracts.csproj", "DevStack.Contracts/"]
COPY ["DevStack.Mcp/DevStack.Mcp.csproj", "DevStack.Mcp/"]
COPY ["DevStack.Tests.Unit/DevStack.Tests.Unit.csproj", "DevStack.Tests.Unit/"]
COPY ["DevStack.Tests.Integration/DevStack.Tests.Integration.csproj", "DevStack.Tests.Integration/"]

COPY . .

RUN dotnet restore DevStack.slnx

# API stage
FROM build AS api
RUN dotnet publish DevStack.Api/DevStack.Api.csproj -c Release -o /app/publish/api --no-restore

# MCP stage
FROM build AS mcp
RUN dotnet publish DevStack.Mcp/DevStack.Mcp.csproj -c Release -o /app/publish/mcp --no-restore

# Runtime stage for API
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime-api
WORKDIR /app

COPY --from=api /app/publish/api .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD dotnet DevStack.Api.dll --health || exit 1

ENTRYPOINT ["dotnet", "DevStack.Api.dll"]

# Runtime stage for MCP
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime-mcp
WORKDIR /app

COPY --from=mcp /app/publish/mcp .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD dotnet DevStack.Mcp.dll --health || exit 1

ENTRYPOINT ["dotnet", "DevStack.Mcp.dll"]