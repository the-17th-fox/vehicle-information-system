FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build-env
WORKDIR /app

COPY Common/ ./Common/
RUN dotnet publish Common/Common.csproj -o out

COPY VehiclesSearchService/ ./VehiclesSearchService/
RUN dotnet publish VehiclesSearchService/VehiclesSearchService.csproj -o out

# final stage/image
FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=build-env /app/out .
ENTRYPOINT ["dotnet", "VehiclesSearchService.dll"]