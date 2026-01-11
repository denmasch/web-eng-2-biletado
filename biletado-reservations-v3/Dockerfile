FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:5207
EXPOSE 5207


FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
ARG BUILD_CONFIGURATION=Release

COPY ["biletado-reservations-v3.csproj", "./"]
RUN dotnet restore "biletado-reservations-v3.csproj"
COPY . .
RUN dotnet build "biletado-reservations-v3.csproj" -c $BUILD_CONFIGURATION -o /app/build


FROM build AS publish
WORKDIR /src
ARG BUILD_CONFIGURATION=Release

RUN dotnet publish "biletado-reservations-v3.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false


FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "biletado-reservations-v3.dll"]


# Development stage: build a dev image using the SDK with hot-reload (dotnet watch).
# Build with: docker build --target dev -t biletado/reservations:dev .
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS dev
WORKDIR /src

ENV ASPNETCORE_ENVIRONMENT=Development
ENV ASPNETCORE_URLS=http://+:5207
EXPOSE 5207

# Copy full source for live edit; when running locally you can mount the host folder to /src for instant changes
COPY . .

# Use dotnet watch to enable hot reload during development
ENTRYPOINT ["dotnet", "watch", "run", "--project", "biletado-reservations-v3.csproj", "--urls", "http://+:5207"]
