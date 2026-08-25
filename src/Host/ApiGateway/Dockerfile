FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project files
COPY src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj src/BuildingBlocks/BuildingBlocks.Common/
COPY src/Host/ApiGateway/ApiGateway.csproj src/Host/ApiGateway/

RUN dotnet restore src/Host/ApiGateway/ApiGateway.csproj

# Copy source code
COPY src/BuildingBlocks/BuildingBlocks.Common/ src/BuildingBlocks/BuildingBlocks.Common/
COPY src/Host/ApiGateway/ src/Host/ApiGateway/

WORKDIR /app/src/Host/ApiGateway
RUN dotnet publish -c Release -o /out

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /out .
ENTRYPOINT ["dotnet", "ApiGateway.dll"]
