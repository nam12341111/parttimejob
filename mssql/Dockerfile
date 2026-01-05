FROM mcr.microsoft.com/mssql/server:2022-latest

# Set environment variables
ENV ACCEPT_EULA=Y \
    MSSQL_SA_PASSWORD=PartTimeJobs@2024

# Copy backup file
COPY hello_2.bak /var/opt/mssql/backup/hello_2.bak

# Copy restore script
COPY mssql/restore-db.sql /var/opt/mssql/scripts/restore-db.sql

# Copy and set entrypoint script
COPY mssql/start.sh /var/opt/mssql/start.sh

# Set the entrypoint
ENTRYPOINT ["/bin/bash", "/var/opt/mssql/start.sh"]
