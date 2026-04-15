# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DevStack.slnx", "."]
COPY ["DevStack.Api/DevStack.Api.csproj", "DevStack.Api/"]
COPY ["DevStack.Application/DevStack.Application.csproj", "DevStack.Application/"]
COPY ["DevStack.Domain/DevStack.Domain.csproj", "DevStack.Domain/"]
COPY ["DevStack.Infrastructure/DevStack.Infrastructure.csproj", "DevStack.Infrastructure/"]
COPY ["DevStack.Contracts/DevStack.Contracts.csproj", "DevStack.Contracts/"]
COPY ["DevStack.Tests.Unit/DevStack.Tests.Unit.csproj", "DevStack.Tests.Unit/"]
COPY ["DevStack.Tests.Integration/DevStack.Tests.Integration.csproj", "DevStack.Tests.Integration/"]

COPY . .

RUN dotnet restore DevStack.slnx

RUN dotnet publish DevStack.Api/DevStack.Api.csproj -c Release -o /app/publish --no-restore

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
  CMD dotnet DevStack.Api.dll --health || exit 1

ENTRYPOINT ["dotnet", "DevStack.Api.dll"]
