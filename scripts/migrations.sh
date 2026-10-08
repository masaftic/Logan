#!/usr/bin/env bash
set -euo pipefail

# Helper script for managing EF Core migrations across microservices

print_usage() {
    echo "Usage: $0 <action> <service> [migration_name] [context_name]"
    echo ""
    echo "Actions:"
    echo "  add <service> <migration_name> [context_name]  Add a new migration"
    echo "  remove <service> [context_name]                Remove the last unapplied migration"
    echo "  list <service> [context_name]                  List migrations for a service"
    echo "  update <service> [context_name]                Apply migrations to the database"
    echo ""
    echo "Available services: Catalog, Inventory, Notification, Ordering, Payment, Shipping"
    echo ""
    echo "Examples:"
    echo "  $0 add Inventory AddStockIndex"
    echo "  $0 update Ordering"
    echo "  $0 list Payment"
    exit 1
}

if [ $# -lt 2 ]; then
    print_usage
fi

ACTION="$1"
SERVICE_INPUT="$2"

# Normalize service name (capitalize first letter)
SERVICE="$(tr '[:lower:]' '[:upper:]' <<< "${SERVICE_INPUT:0:1}")$(tr '[:upper:]' '[:lower:]' <<< "${SERVICE_INPUT:1}")"
PROJECT_DIR="src/Services/${SERVICE}/${SERVICE}.Api"

if [ ! -d "$PROJECT_DIR" ]; then
    echo "Error: Service project directory '$PROJECT_DIR' does not exist." >&2
    echo "Available services: Catalog, Inventory, Notification, Ordering, Payment, Shipping" >&2
    exit 1
fi

# Default context name mapping
get_default_context() {
    case "$SERVICE" in
        Catalog) echo "CatalogDbContext" ;;
        Inventory) echo "InventoryDbContext" ;;
        Notification) echo "NotificationDbContext" ;;
        Ordering) echo "OrderDbContext" ;;
        Payment) echo "PaymentDbContext" ;;
        Shipping) echo "ShippingDbContext" ;;
        *) echo "${SERVICE}DbContext" ;;
    esac
}

DEFAULT_CONTEXT="$(get_default_context)"

case "$ACTION" in
    add)
        if [ $# -lt 3 ]; then
            echo "Error: Migration name required for 'add' action." >&2
            echo "Example: $0 add $SERVICE InitialCreate" >&2
            exit 1
        fi
        MIGRATION_NAME="$3"
        CONTEXT_NAME="${4:-$DEFAULT_CONTEXT}"
        echo "==> Adding migration '$MIGRATION_NAME' to $SERVICE service ($CONTEXT_NAME)..."
        dotnet ef migrations add "$MIGRATION_NAME" \
            --project "$PROJECT_DIR" \
            --startup-project "$PROJECT_DIR" \
            --context "$CONTEXT_NAME" \
            --output-dir Data/Migrations
        ;;

    remove)
        CONTEXT_NAME="${3:-$DEFAULT_CONTEXT}"
        echo "==> Removing last migration from $SERVICE service ($CONTEXT_NAME)..."
        dotnet ef migrations remove \
            --project "$PROJECT_DIR" \
            --startup-project "$PROJECT_DIR" \
            --context "$CONTEXT_NAME" \
            --force
        ;;

    list)
        CONTEXT_NAME="${3:-$DEFAULT_CONTEXT}"
        echo "==> Listing migrations for $SERVICE service ($CONTEXT_NAME)..."
        dotnet ef migrations list \
            --project "$PROJECT_DIR" \
            --startup-project "$PROJECT_DIR" \
            --context "$CONTEXT_NAME"
        ;;

    update)
        CONTEXT_NAME="${3:-$DEFAULT_CONTEXT}"
        echo "==> Applying database migrations for $SERVICE service ($CONTEXT_NAME)..."
        dotnet ef database update \
            --project "$PROJECT_DIR" \
            --startup-project "$PROJECT_DIR" \
            --context "$CONTEXT_NAME"
        ;;

    *)
        echo "Error: Unknown action '$ACTION'" >&2
        print_usage
        ;;
esac
