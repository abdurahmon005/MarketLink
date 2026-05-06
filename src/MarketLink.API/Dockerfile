FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# COPY host dagi src/ ? konteyner dagi /src/ ga
COPY ["src/MarketLink.API/MarketLink.API.csproj", "MarketLink.API/"]
COPY ["src/MarketLink.Application/MarketLink.Application.csproj", "MarketLink.Application/"]
COPY ["src/MarketLink.DataAccess/MarketLink.DataAccess.csproj", "MarketLink.DataAccess/"]
COPY ["src/MarketLink.Domain/MarketLink.Domain.csproj", "MarketLink.Domain/"]

RUN dotnet restore "MarketLink.API/MarketLink.API.csproj"

COPY src/ .

RUN dotnet publish "MarketLink.API/MarketLink.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MarketLink.API.dll"]