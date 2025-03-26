# Base image for the final container
FROM mcr.microsoft.com/dotnet/runtime:9.0-alpine AS base
WORKDIR /app

# Build image
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
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
    -p:PublishReadyToRun=true

# Final image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Set non-root user for better security
# Creating user inside container rather than using $APP_UID since Alpine uses different user management
RUN adduser -D -h /app abyssirc && \
    chown -R abyssirc:abyssirc /app
USER abyssirc

# Create directories for data persistence
RUN mkdir -p /app/data /app/logs /app/scripts

ENTRYPOINT ["./AbyssIrc.Server"]
