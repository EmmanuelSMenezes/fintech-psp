FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["src/Services/FintechPSP.WebhookService/FintechPSP.WebhookService.csproj", "src/Services/FintechPSP.WebhookService/"]
COPY ["src/Shared/FintechPSP.Shared.Domain/FintechPSP.Shared.Domain.csproj", "src/Shared/FintechPSP.Shared.Domain/"]
COPY ["src/Shared/FintechPSP.Shared.Infrastructure/FintechPSP.Shared.Infrastructure.csproj", "src/Shared/FintechPSP.Shared.Infrastructure/"]
RUN dotnet restore "src/Services/FintechPSP.WebhookService/FintechPSP.WebhookService.csproj"
COPY . .
WORKDIR "/src/src/Services/FintechPSP.WebhookService"
RUN dotnet build "FintechPSP.WebhookService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FintechPSP.WebhookService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FintechPSP.WebhookService.dll"]
