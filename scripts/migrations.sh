#!/usr/bin/env bash
set -euo pipefail

# Helper script for managing EF Core migrations across microservices

print_usage() {
    echo "Usage: $0 <action> <service> [migration_name] <context_name>"
    echo ""
    echo "Actions:"
    echo "  add <service> <migration_name> <context_name>  Add a new migration"
    echo "  remove <service> <context_name>                Remove the last unapplied migration"
    echo "  list <service> <context_name>                  List migrations for a service"
    echo "  update <service> <context_name>                Apply migrations to the database"
    echo ""
    echo "Available services: Inventory, Ordering, Payments, Fulfillment"
    echo ""
    echo "Examples:"
    echo "  $0 add Inventory InitialCreate InventoryDbContext"
    echo "  $0 remove Inventory InventoryDbContext"
    echo "  $0 list Inventory InventoryDbContext"
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
CONTEXT_NAME="$4"

if [ ! -d "$PROJECT_DIR" ]; then
    echo "Error: Service project directory '$PROJECT_DIR' does not exist." >&2
    exit 1
fi

case "$ACTION" in
    add)
        if [ $# -lt 3 ]; then
            echo "Error: Migration name required for 'add' action." >&2
            echo "Example: $0 add $SERVICE InitialCreate" >&2
            exit 1
        fi
        MIGRATION_NAME="$3"
        echo "==> Adding migration '$MIGRATION_NAME' to $SERVICE service ($CONTEXT_NAME)..."
        dotnet ef migrations add "$MIGRATION_NAME" \
            --project "$PROJECT_DIR" \
            --startup-project "$PROJECT_DIR" \
            --context "$CONTEXT_NAME" \
            --output-dir Data/Migrations
        ;;

    remove)
        echo "==> Removing last migration from $SERVICE service ($CONTEXT_NAME)..."
        dotnet ef migrations remove \
            --project "$PROJECT_DIR" \
            --startup-project "$PROJECT_DIR" \
            --context "$CONTEXT_NAME" \
            --force
        ;;

    list)
        echo "==> Listing migrations for $SERVICE service ($CONTEXT_NAME)..."
        dotnet ef migrations list \
            --project "$PROJECT_DIR" \
            --startup-project "$PROJECT_DIR" \
            --context "$CONTEXT_NAME"
        ;;

    update)
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

