/* ---------------------------------------------------------------------------
   Tender Management API  –  create_login.sql
   Run while connected to the master database (sqlcmd -d master -i create_login.sql)
--------------------------------------------------------------------------- */

/* ---------- 1. Create empty DB if it doesn't exist ----------------------- */
IF DB_ID(N'TenderDb') IS NULL
BEGIN
    DECLARE @DataFolder nvarchar(4000) = CAST(SERVERPROPERTY('InstanceDefaultDataPath') AS nvarchar(4000));
    DECLARE @LogFolder  nvarchar(4000) = CAST(SERVERPROPERTY('InstanceDefaultLogPath')  AS nvarchar(4000));

    DECLARE @dataFile nvarchar(4000) = @DataFolder + N'TenderDb.mdf';
    DECLARE @logFile  nvarchar(4000) = @LogFolder  + N'TenderDb_log.ldf';

    PRINT 'Creating database TenderDb at:';
    PRINT @dataFile;
    PRINT @logFile;

    EXEC ('CREATE DATABASE TenderDb
           ON  (NAME = TenderDb,     FILENAME = ''' + @dataFile + ''')
           LOG ON (NAME = TenderDb_log, FILENAME = ''' + @logFile + ''')
           COLLATE SQL_Latin1_General_CP1_CI_AS');
END
GO


/* ---------- 2. Grant application access  ---------------------------------

   Pick ONE model.  Leave it uncommented and comment the other block.        */

/* === Integrated Security (recommended when IIS and SQL are on same box) === */
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'IIS APPPOOL\TenderApi')
    CREATE LOGIN [IIS APPPOOL\TenderApi] FROM WINDOWS;
GO
USE TenderDb;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'IIS APPPOOL\TenderApi')
    CREATE USER  [IIS APPPOOL\TenderApi] FOR LOGIN [IIS APPPOOL\TenderApi];
EXEC sp_addrolemember N'db_datareader', [IIS APPPOOL\TenderApi];
EXEC sp_addrolemember N'db_datawriter', [IIS APPPOOL\TenderApi];
-- EXEC sp_addrolemember N'db_ddladmin',  [IIS APPPOOL\TenderApi];  -- ← uncomment if migrations will run under app-pool identity
GO

/*
-- === SQL Authentication (use if SQL is remote or you prefer explicit creds) ===
IF NOT EXISTS (SELECT 1 FROM sys.server_principals WHERE name = N'tender_api_user')
    CREATE LOGIN tender_api_user WITH PASSWORD = 'UltraLong$tr0ngPW!';
GO
USE TenderDb;
GO
IF NOT EXISTS (SELECT 1 FROM sys.database_principals WHERE name = N'tender_api_user')
    CREATE USER  tender_api_user FOR LOGIN tender_api_user;
EXEC sp_addrolemember N'db_datareader', tender_api_user;
EXEC sp_addrolemember N'db_datawriter', tender_api_user;
-- EXEC sp_addrolemember N'db_ddladmin',  tender_api_user;          -- ← uncomment if migrations will run under SQL login
GO
*/

PRINT 'Database and login/user creation complete.';
