FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/FintechPSP.TransactionService/FintechPSP.TransactionService.csproj", "src/Services/FintechPSP.TransactionService/"]
COPY ["src/Shared/FintechPSP.Shared.Domain/FintechPSP.Shared.Domain.csproj", "src/Shared/FintechPSP.Shared.Domain/"]
COPY ["src/Shared/FintechPSP.Shared.Infrastructure/FintechPSP.Shared.Infrastructure.csproj", "src/Shared/FintechPSP.Shared.Infrastructure/"]
RUN dotnet restore "src/Services/FintechPSP.TransactionService/FintechPSP.TransactionService.csproj"
COPY . .
WORKDIR "/src/src/Services/FintechPSP.TransactionService"
RUN dotnet build "FintechPSP.TransactionService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FintechPSP.TransactionService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FintechPSP.TransactionService.dll"]
