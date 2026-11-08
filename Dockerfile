FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Build.targets nuget.config ./
COPY src/FurBot/FurBot.csproj src/FurBot/
RUN dotnet restore src/FurBot/FurBot.csproj

COPY src/FurBot/ src/FurBot/
RUN dotnet publish src/FurBot/FurBot.csproj --configuration Release --no-restore --output /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/runtime:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish/ ./
RUN mkdir logs && chown -R app:app /app
USER app
ENTRYPOINT ["dotnet", "FurBot.dll"]
