#!/usr/bin/env bash
set -e

# Creates dedicated databases for services if separate database connections are used
psql -v ON_ERROR_STOP=1 --username "$POSTGRES_USER" --dbname "$POSTGRES_DB" <<-EOSQL
    CREATE DATABASE payments_db;
    CREATE DATABASE shipping_db;
    CREATE DATABASE catalog_db;
EOSQL
