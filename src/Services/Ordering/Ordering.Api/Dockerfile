FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project files
COPY src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj src/BuildingBlocks/BuildingBlocks.Common/
COPY src/Services/Ordering/Ordering.Contracts/Ordering.Contracts.csproj src/Services/Ordering/Ordering.Contracts/
COPY src/Services/Ordering/Ordering.Api/Ordering.Api.csproj src/Services/Ordering/Ordering.Api/

RUN dotnet restore src/Services/Ordering/Ordering.Api/Ordering.Api.csproj

# Copy source code
COPY src/BuildingBlocks/BuildingBlocks.Common/ src/BuildingBlocks/BuildingBlocks.Common/
COPY src/Services/Ordering/Ordering.Contracts/ src/Services/Ordering/Ordering.Contracts/
COPY src/Services/Ordering/Ordering.Api/ src/Services/Ordering/Ordering.Api/

WORKDIR /app/src/Services/Ordering/Ordering.Api
RUN dotnet publish -c Release -o /out

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Ordering.Api.dll"]
