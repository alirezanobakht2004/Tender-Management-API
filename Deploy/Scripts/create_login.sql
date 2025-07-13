-- create_login.sql
-- Run against the master database.

-- 1. Create TenderDb if missing
IF DB_ID(N'TenderDb') IS NULL
BEGIN
    DECLARE @dataFolder nvarchar(4000) = CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000));
    DECLARE @logFolder  nvarchar(4000) = CAST(SERVERPROPERTY('InstanceDefaultLogPath')  AS nvarchar(4000));
    DECLARE @dataFile nvarchar(4000) = @dataFolder + N'TenderDb.mdf';
    DECLARE @logFile  nvarchar(4000) = @logFolder  + N'TenderDb_log.ldf';

    EXEC ('CREATE DATABASE TenderDb
           ON  (NAME = TenderDb,     FILENAME = ''' + @dataFile + ''')
           LOG ON (NAME = TenderDb_log, FILENAME = ''' + @logFile + ''')');
END
GO

-- 2. Grant access – choose ONE block:

-- A) Integrated Security
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'IIS APPPOOL\TenderApi')
    CREATE LOGIN [IIS APPPOOL\TenderApi] FROM WINDOWS;
GO
USE TenderDb;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'IIS APPPOOL\TenderApi')
    CREATE USER [IIS APPPOOL\TenderApi] FOR LOGIN [IIS APPPOOL\TenderApi];
EXEC sp_addrolemember N'db_datareader', [IIS APPPOOL\TenderApi];
EXEC sp_addrolemember N'db_datawriter', [IIS APPPOOL\TenderApi];
GO

/*
-- B) SQL Authentication
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'tender_api_user')
    CREATE LOGIN tender_api_user WITH PASSWORD = 'UltraLong$tr0ngPW!';
GO
USE TenderDb;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'tender_api_user')
    CREATE USER tender_api_user FOR LOGIN tender_api_user;
EXEC sp_addrolemember N'db_datareader', tender_api_user;
EXEC sp_addrolemember N'db_datawriter', tender_api_user;
GO
*/

PRINT 'Database and login/user creation complete.';

