FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project files
COPY src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj src/BuildingBlocks/BuildingBlocks.Common/
COPY src/BuildingBlocks/BuildingBlocks.Persistence/BuildingBlocks.Persistence.csproj src/BuildingBlocks/BuildingBlocks.Persistence/
COPY src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/Services/Catalog/Catalog.Contracts/Catalog.Contracts.csproj src/Services/Catalog/Catalog.Contracts/
COPY src/Services/Catalog/Catalog.Api/Catalog.Api.csproj src/Services/Catalog/Catalog.Api/

RUN dotnet restore src/Services/Catalog/Catalog.Api/Catalog.Api.csproj

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Catalog/ src/Services/Catalog/

WORKDIR /app/src/Services/Catalog/Catalog.Api
RUN dotnet publish -c Release -o /out

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Catalog.Api.dll"]
