-- Restore database from hello_2.bak with MOVE to Linux paths
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'PartTimeJobs')
BEGIN
    ALTER DATABASE PartTimeJobs SET SINGLE_USER WITH ROLLBACK IMMEDIATE
    DROP DATABASE PartTimeJobs
END
GO

RESTORE DATABASE PartTimeJobs
FROM DISK = '/var/opt/mssql/backup/hello_2.bak'
WITH 
    REPLACE,
    RECOVERY,
    MOVE 'PartTimeJobs' TO '/var/opt/mssql/data/PartTimeJobs.mdf',
    MOVE 'PartTimeJobs_log' TO '/var/opt/mssql/data/PartTimeJobs_log.ldf'
GO

-- Check if restore was successful
IF EXISTS (SELECT 1 FROM sys.databases WHERE name = 'PartTimeJobs')
BEGIN
    PRINT 'Database PartTimeJobs restored successfully'
END
ELSE
BEGIN
    PRINT 'Database restore failed'
END
