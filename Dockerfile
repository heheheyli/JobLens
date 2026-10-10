# syntax=docker/dockerfile:1

# ---------- Stage 1: build (has the SDK, never shipped) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first. This layer is cached until the .csproj changes,
# which keeps rebuilds fast (helps the < 6 min build target).
COPY JobLens.Web/JobLens.Web.csproj JobLens.Web/
RUN dotnet restore JobLens.Web/JobLens.Web.csproj

# Copy the rest of the source and publish a release build
COPY . .
RUN dotnet publish JobLens.Web/JobLens.Web.csproj \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# ---------- Stage 2: runtime (only ASP.NET runtime + published files) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Passed in by the GitHub Actions workflow so the Version page can show
# which commit is running (used in the pipeline demo).
ARG BUILD_COMMIT=unknown
ARG BUILD_TIME=unknown
ENV BUILD_COMMIT=$BUILD_COMMIT \
    BUILD_TIME=$BUILD_TIME \
    ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

# Run as the non-root "app" user that ships with the .NET images
USER $APP_UID

# Assembly name defaults to the project name (JobLens.Web)
ENTRYPOINT ["dotnet", "JobLens.Web.dll"]
