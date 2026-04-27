FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/Recommendation.ETL/Recommendation.ETL.csproj", "src/Recommendation.ETL/"]
COPY ["src/Recommendation.Data/Recommendation.Data.csproj", "src/Recommendation.Data/"]
COPY ["src/Recommendation.Shared/Recommendation.Shared.csproj", "src/Recommendation.Shared/"]
RUN dotnet restore "src/Recommendation.ETL/Recommendation.ETL.csproj"
COPY . .
RUN dotnet build "src/Recommendation.ETL/Recommendation.ETL.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "src/Recommendation.ETL/Recommendation.ETL.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Recommendation.ETL.dll"]
