# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Server/DevStack.slnx", "src/Server/"]
COPY ["src/Server/DevStack.Api/DevStack.Api.csproj", "src/Server/DevStack.Api/"]
COPY ["src/Server/DevStack.Application/DevStack.Application.csproj", "src/Server/DevStack.Application/"]
COPY ["src/Server/DevStack.Domain/DevStack.Domain.csproj", "src/Server/DevStack.Domain/"]
COPY ["src/Server/DevStack.Infrastructure/DevStack.Infrastructure.csproj", "src/Server/DevStack.Infrastructure/"]
COPY ["src/Server/DevStack.Contracts/DevStack.Contracts.csproj", "src/Server/DevStack.Contracts/"]
COPY ["src/Server/DevStack.Tests.Unit/DevStack.Tests.Unit.csproj", "src/Server/DevStack.Tests.Unit/"]
COPY ["src/Server/DevStack.Tests.Integration/DevStack.Tests.Integration.csproj", "src/Server/DevStack.Tests.Integration/"]

RUN dotnet restore src/Server/DevStack.slnx

COPY . .
RUN dotnet publish src/Server/DevStack.Api/DevStack.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD dotnet DevStack.Api.dll --health || exit 1

ENTRYPOINT ["dotnet", "DevStack.Api.dll"]
