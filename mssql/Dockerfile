FROM mcr.microsoft.com/mssql/server:2022-latest

# Set environment variables
ENV ACCEPT_EULA=Y \
    MSSQL_SA_PASSWORD=PartTimeJobs@2024

# Copy backup file
COPY hello_2.bak /var/opt/mssql/backup/hello_2.bak

# Copy restore script
COPY mssql/restore-db.sql /var/opt/mssql/scripts/restore-db.sql

# Create entrypoint script with correct permissions
RUN printf '#!/bin/bash\nset -e\n\n/opt/mssql/bin/sqlservr &\nSERdcVER_PID=$!\n\necho "Waiting for SQL Server to be ready..."\nRETRY_COUNT=0\nMAX_RETRIES=60\n\nwhile [ $RETRY_COUNT -lt $MAX_RETRIES ]; do\n  /opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "PartTimeJobs@2024" -C -Q "SELECT 1" > /dev/null 2>&1\n  if [ $? -eq 0 ]; then\n    echo "SQL Server is ready!"\n    break\n  fi\n  RETRY_COUNT=$((RETRY_COUNT + 1))\n  echo "Waiting... attempt $RETRY_COUNT/$MAX_RETRIES"\n  sleep 1\ndone\n\nif [ $RETRY_COUNT -eq $MAX_RETRIES ]; then\n  echo "SQL Server failed to start"\n  kill $SERVER_PID\n  exit 1\nfi\n\necho "Restoring database from hello_2.bak..."\n/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "PartTimeJobs@2024" -C -i /var/opt/mssql/scripts/restore-db.sql\n\necho "Database restore completed!"\n\nwait $SERVER_PID' > /entrypoint.sh

# Set the entrypoint
ENTRYPOINT ["/bin/bash", "/entrypoint.sh"]
