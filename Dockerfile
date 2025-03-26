# Base image for the final container
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
USER $APP_UID
WORKDIR /app

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
WORKDIR /src
COPY ["src/AbyssIrc.Server/AbyssIrc.Server.csproj", "src/AbyssIrc.Server/"]
RUN dotnet restore "src/AbyssIrc.Server/AbyssIrc.Server.csproj" -a $TARGETARCH
COPY . .
WORKDIR "/src/src/AbyssIrc.Server"
RUN dotnet build "AbyssIrc.Server.csproj" -c $BUILD_CONFIGURATION -o /app/build -a $TARGETARCH

# Publish image with single file
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
ARG TARGETARCH
RUN dotnet publish "AbyssIrc.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish \
    -a $TARGETARCH \
    -p:PublishSingleFile=true \
    -p:PublishReadyToRun=true \
    -p:PublishTrimmed=true \
    -p:InvariantGlobalization=true

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["./AbyssIrc.Server"]
