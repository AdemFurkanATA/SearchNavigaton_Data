FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Recommendation.API/Recommendation.API.csproj", "src/Recommendation.API/"]
COPY ["src/Recommendation.Data/Recommendation.Data.csproj", "src/Recommendation.Data/"]
COPY ["src/Recommendation.Shared/Recommendation.Shared.csproj", "src/Recommendation.Shared/"]
RUN dotnet restore "src/Recommendation.API/Recommendation.API.csproj"
COPY . .
RUN dotnet build "src/Recommendation.API/Recommendation.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Recommendation.API/Recommendation.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Recommendation.API.dll"]
