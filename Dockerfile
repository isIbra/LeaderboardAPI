FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["Leaderboard.API/Leaderboard.API.csproj", "Leaderboard.API/"]

RUN dotnet restore "Leaderboard.API/Leaderboard.API.csproj"

COPY . .

RUN dotnet build "Leaderboard.API/Leaderboard.API.csproj" -c Release -o /app/build

RUN dotnet publish "Leaderboard.API/Leaderboard.API.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runner
WORKDIR /app
EXPOSE 8080
EXPOSE 443

COPY --from=build /app/publish .

# Create a non-root user to run the app

RUN useradd -m appuser
USER appuser

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Leaderboard.API.dll"]
