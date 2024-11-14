# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /source

# copy csproj and restore as distinct layers
COPY *.sln .
COPY GeoFilterService/*.csproj ./GeoFilterService/
RUN dotnet restore

# copy everything else and build app
COPY GeoFilterService/. ./GeoFilterService/
WORKDIR /source/GeoFilterService
RUN dotnet publish -c release -o /app --no-restore --runtime ubuntu.24.04-x64

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app ./
ENTRYPOINT ["dotnet", "GeoFilterService.dll" "--urls=http://0.0.0.0:8080"]