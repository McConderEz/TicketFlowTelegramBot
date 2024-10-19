FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TelegramBotCode418Service/TelegramBotCode418Service.csproj", "TelegramBotCode418Service/"]
RUN dotnet restore "TelegramBotCode418Service/TelegramBotCode418Service.csproj"
COPY . .
WORKDIR "/src/TelegramBotCode418Service"
RUN dotnet build "TelegramBotCode418Service.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "TelegramBotCode418Service.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TelegramBotCode418Service.dll"]
