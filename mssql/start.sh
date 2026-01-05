#!/bin/bash
set -e

/opt/mssql/bin/sqlservr &
SERVER_PID=$!

echo "Waiting for SQL Server to be ready..."
RETRY_COUNT=0
MAX_RETRIES=60

while [ $RETRY_COUNT -lt $MAX_RETRIES ]; do
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "PartTimeJobs@2024" -C -Q "SELECT 1" > /dev/null 2>&1
  if [ $? -eq 0 ]; then
    echo "SQL Server is ready!"
    break
  fi
  RETRY_COUNT=$((RETRY_COUNT + 1))
  echo "Waiting... attempt $RETRY_COUNT/$MAX_RETRIES"
  sleep 1
done

if [ $RETRY_COUNT -eq $MAX_RETRIES ]; then
  echo "SQL Server failed to start"
  kill $SERVER_PID
  exit 1
fi

echo "Restoring database from hello_2.bak..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "PartTimeJobs@2024" -C -i /var/opt/mssql/scripts/restore-db.sql

echo "Database restore completed!"

wait $SERVER_PID
