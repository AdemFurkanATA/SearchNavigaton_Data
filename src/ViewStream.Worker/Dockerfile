FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/ViewStream.Worker/ViewStream.Worker.csproj", "src/ViewStream.Worker/"]
COPY ["src/Recommendation.Shared/Recommendation.Shared.csproj", "src/Recommendation.Shared/"]
RUN dotnet restore "src/ViewStream.Worker/ViewStream.Worker.csproj"
COPY . .
RUN dotnet build "src/ViewStream.Worker/ViewStream.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/ViewStream.Worker/ViewStream.Worker.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ViewStream.Worker.dll"]
