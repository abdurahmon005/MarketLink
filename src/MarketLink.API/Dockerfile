FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/MarketLink.API/MarketLink.API.csproj", "src/MarketLink.API/"]
COPY ["src/MarketLink.Application/MarketLink.Application.csproj", "src/MarketLink.Application/"]
COPY ["src/MarketLink.DataAccess/MarketLink.DataAccess.csproj", "src/MarketLink.DataAccess/"]
COPY ["src/MarketLink.Domain/MarketLink.Domain.csproj", "src/MarketLink.Domain/"]
RUN dotnet restore "src/MarketLink.API/MarketLink.API.csproj"
COPY . .
RUN dotnet publish "src/MarketLink.API/MarketLink.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "MarketLink.API.dll"]