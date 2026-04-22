FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DevStack.slnx", "."]
COPY ["DevStack.Api/DevStack.Api.csproj", "DevStack.Api/"]
COPY ["DevStack.Application/DevStack.Application.csproj", "DevStack.Application/"]
COPY ["DevStack.Domain/DevStack.Domain.csproj", "DevStack.Domain/"]
COPY ["DevStack.Infrastructure/DevStack.Infrastructure.csproj", "DevStack.Infrastructure/"]
COPY ["DevStack.Persistence/DevStack.Persistence.csproj", "DevStack.Persistence/"]
COPY ["DevStack.Contracts/DevStack.Contracts.csproj", "DevStack.Contracts/"]

COPY . .

RUN dotnet restore DevStack.slnx
RUN dotnet publish DevStack.Api/DevStack.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

RUN apt-get update && apt-get install -y --no-install-recommends curl && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "DevStack.Api.dll"]