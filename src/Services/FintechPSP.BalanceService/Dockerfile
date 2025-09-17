FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/FintechPSP.BalanceService/FintechPSP.BalanceService.csproj", "src/Services/FintechPSP.BalanceService/"]
COPY ["src/Shared/FintechPSP.Shared.Domain/FintechPSP.Shared.Domain.csproj", "src/Shared/FintechPSP.Shared.Domain/"]
COPY ["src/Shared/FintechPSP.Shared.Infrastructure/FintechPSP.Shared.Infrastructure.csproj", "src/Shared/FintechPSP.Shared.Infrastructure/"]
RUN dotnet restore "src/Services/FintechPSP.BalanceService/FintechPSP.BalanceService.csproj"
COPY . .
WORKDIR "/src/src/Services/FintechPSP.BalanceService"
RUN dotnet build "FintechPSP.BalanceService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FintechPSP.BalanceService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FintechPSP.BalanceService.dll"]
