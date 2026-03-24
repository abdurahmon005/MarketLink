FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MarketLink.API/MarketLink.API.csproj",         "MarketLink.API/"]
COPY ["MarketLink.Application/MarketLink.Application.csproj", "MarketLink.Application/"]
COPY ["MarketLink.DataAccess/MarketLink.DataAccess.csproj",   "MarketLink.DataAccess/"]
COPY ["MarketLink.Domain/MarketLink.Domain.csproj",           "MarketLink.Domain/"]

RUN dotnet restore "MarketLink.API/MarketLink.API.csproj"

COPY . .
WORKDIR "/src/MarketLink.API"
RUN dotnet build "MarketLink.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "MarketLink.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "MarketLink.API.dll"]
