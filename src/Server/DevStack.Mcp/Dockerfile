FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["DevStack.slnx", "."]
COPY ["DevStack.Mcp/DevStack.Mcp.csproj", "DevStack.Mcp/"]
COPY ["DevStack.Application/DevStack.Application.csproj", "DevStack.Application/"]
COPY ["DevStack.Domain/DevStack.Domain.csproj", "DevStack.Domain/"]
COPY ["DevStack.Infrastructure/DevStack.Infrastructure.csproj", "DevStack.Infrastructure/"]
COPY ["DevStack.Persistence/DevStack.Persistence.csproj", "DevStack.Persistence/"]
COPY ["DevStack.Contracts/DevStack.Contracts.csproj", "DevStack.Contracts/"]

COPY . .

RUN dotnet restore DevStack.slnx
RUN dotnet publish DevStack.Mcp/DevStack.Mcp.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "DevStack.Mcp.dll"]
