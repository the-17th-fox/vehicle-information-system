# https://hub.docker.com/_/microsoft-dotnet
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY Common/Common.csproj ./Common/
RUN dotnet restore Common/Common.csproj

COPY Common/ ./Common/
RUN dotnet publish Common/Common.csproj -o out

COPY LogsViewerService/*.csproj ./LogsViewerService/
RUN dotnet restore LogsViewerService/LogsViewerService.csproj

COPY LogsViewerService/ ./LogsViewerService/
RUN dotnet publish LogsViewerService/LogsViewerService.csproj -o out

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "LogsViewerService.dll"]