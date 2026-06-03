#!/usr/bin/env bash
# Seeds the SQL Server container with infra-local/sql/init.sql.
# Requires the stack to be running: docker compose up -d
set -euo pipefail
HERE="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
******

echo "Waiting for SQL Server to accept connections..."
for i in $(seq 1 30); do
  if docker exec aff-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$PASSWORD" -C -Q "SELECT 1" >/dev/null 2>&1; then
    break
  fi
  sleep 2
done

echo "Applying init.sql..."
docker cp "$HERE/../sql/init.sql" aff-sqlserver:/tmp/init.sql
docker exec aff-sqlserver /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$PASSWORD" -C -i /tmp/init.sql
echo "Done. LearningDb.dbo.Customers is seeded."
