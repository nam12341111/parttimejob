#!/bin/bash

# Wait for SQL Server to be ready
echo "Waiting for SQL Server to be ready..."
for i in {1..50}; do
  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "PartTimeJobs@2024" -C -Q "SELECT 1" > /dev/null 2>&1
  if [ $? -eq 0 ]; then
    echo "SQL Server is ready!"
    break
  fi
  echo "Waiting... attempt $i/50"
  sleep 1
done

# Run the restore script
echo "Restoring database from hello_2.bak..."
/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "PartTimeJobs@2024" -C -i /var/opt/mssql/scripts/restore-db.sql

echo "Database restore completed!"