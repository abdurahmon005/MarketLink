# ── Build stage ──────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MarketLink.API/MarketLink.API.csproj",                 "MarketLink.API/"]
COPY ["MarketLink.Application/MarketLink.Application.csproj", "MarketLink.Application/"]
COPY ["MarketLink.DataAccess/MarketLink.DataAccess.csproj",   "MarketLink.DataAccess/"]
COPY ["MarketLink.Domain/MarketLink.Domain.csproj",           "MarketLink.Domain/"]

RUN dotnet restore "MarketLink.API/MarketLink.API.csproj"

COPY . .
WORKDIR "/src/MarketLink.API"
RUN dotnet publish "MarketLink.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# ── Runtime stage ─────────────────────────────────────────────
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Non-root user (xavfsizlik)
RUN adduser --disabled-password --gecos "" appuser && chown -R appuser /app
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MarketLink.API.dll"]
