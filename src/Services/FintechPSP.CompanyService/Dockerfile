FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Services/FintechPSP.CompanyService/FintechPSP.CompanyService.csproj", "src/Services/FintechPSP.CompanyService/"]
RUN dotnet restore "src/Services/FintechPSP.CompanyService/FintechPSP.CompanyService.csproj"
COPY . .
WORKDIR "/src/src/Services/FintechPSP.CompanyService"
RUN dotnet build "FintechPSP.CompanyService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FintechPSP.CompanyService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FintechPSP.CompanyService.dll"]

