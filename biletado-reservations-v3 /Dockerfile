FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["biletado-reservations-v3 /biletado-reservations-v3.csproj", "biletado-reservations-v3 /"]
RUN dotnet restore "biletado-reservations-v3 /biletado-reservations-v3.csproj"
COPY . .
WORKDIR "/src/biletado-reservations-v3 "
RUN dotnet build "./biletado-reservations-v3.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./biletado-reservations-v3.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "biletado-reservations-v3.dll"]
