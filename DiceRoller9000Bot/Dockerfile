#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:3.1-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["DiceRoller9000Bot/DiceRoller9000Bot.csproj", "DiceRoller9000Bot/"]
RUN dotnet restore "DiceRoller9000Bot/DiceRoller9000Bot.csproj" --source https://api.nuget.org/v3/index.json --source https://nuget.emzi0767.com/api/v3/index.json
COPY . .
WORKDIR "/src/DiceRoller9000Bot"
RUN dotnet build "DiceRoller9000Bot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DiceRoller9000Bot.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DiceRoller9000Bot.dll"]