FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/AbyssIrc.Server/AbyssIrc.Server.csproj", "src/AbyssIrc.Server/"]
RUN dotnet restore "src/AbyssIrc.Server/AbyssIrc.Server.csproj"
COPY . .
WORKDIR "/src/src/AbyssIrc.Server"
RUN dotnet build "AbyssIrc.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "AbyssIrc.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "AbyssIrc.Server.dll"]
