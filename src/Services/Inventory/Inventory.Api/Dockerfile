FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project files
COPY src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj src/BuildingBlocks/BuildingBlocks.Common/
COPY src/Services/Inventory/Inventory.Contracts/Inventory.Contracts.csproj src/Services/Inventory/Inventory.Contracts/
COPY src/Services/Inventory/Inventory.Api/Inventory.Api.csproj src/Services/Inventory/Inventory.Api/

RUN dotnet restore src/Services/Inventory/Inventory.Api/Inventory.Api.csproj

# Copy source code
COPY src/BuildingBlocks/BuildingBlocks.Common/ src/BuildingBlocks/BuildingBlocks.Common/
COPY src/Services/Inventory/Inventory.Contracts/ src/Services/Inventory/Inventory.Contracts/
COPY src/Services/Inventory/Inventory.Api/ src/Services/Inventory/Inventory.Api/

WORKDIR /app/src/Services/Inventory/Inventory.Api
RUN dotnet publish -c Release -o /out

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Inventory.Api.dll"]
