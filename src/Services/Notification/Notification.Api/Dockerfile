FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project files
COPY src/BuildingBlocks/BuildingBlocks.Common/BuildingBlocks.Common.csproj src/BuildingBlocks/BuildingBlocks.Common/
COPY src/BuildingBlocks/BuildingBlocks.Persistence/BuildingBlocks.Persistence.csproj src/BuildingBlocks/BuildingBlocks.Persistence/
COPY src/BuildingBlocks/BuildingBlocks.Messaging/BuildingBlocks.Messaging.csproj src/BuildingBlocks/BuildingBlocks.Messaging/
COPY src/Services/Notification/Notification.Contracts/Notification.Contracts.csproj src/Services/Notification/Notification.Contracts/
COPY src/Services/Ordering/Ordering.Contracts/Ordering.Contracts.csproj src/Services/Ordering/Ordering.Contracts/
COPY src/Services/Payment/Payment.Contracts/Payment.Contracts.csproj src/Services/Payment/Payment.Contracts/
COPY src/Services/Shipping/Shipping.Contracts/Shipping.Contracts.csproj src/Services/Shipping/Shipping.Contracts/
COPY src/Services/Notification/Notification.Api/Notification.Api.csproj src/Services/Notification/Notification.Api/

RUN dotnet restore src/Services/Notification/Notification.Api/Notification.Api.csproj

# Copy source code
COPY src/BuildingBlocks/ src/BuildingBlocks/
COPY src/Services/Notification/ src/Services/Notification/
COPY src/Services/Ordering/Ordering.Contracts/ src/Services/Ordering/Ordering.Contracts/
COPY src/Services/Payment/Payment.Contracts/ src/Services/Payment/Payment.Contracts/
COPY src/Services/Shipping/Shipping.Contracts/ src/Services/Shipping/Shipping.Contracts/

WORKDIR /app/src/Services/Notification/Notification.Api
RUN dotnet publish -c Release -o /out

# Final runtime image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
COPY --from=build /out .
ENTRYPOINT ["dotnet", "Notification.Api.dll"]
