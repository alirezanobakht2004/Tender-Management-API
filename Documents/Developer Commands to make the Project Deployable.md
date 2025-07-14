# Developer Commands to Make the Project Deployable

1) ### Publish the project, from the solution root(TenderManagement Folder), run:

dotnet publish Tender.Api/Tender.Api.csproj -c Release -o ../Deploy/Site

2) ### Find Deploy/Site/web.config and Make it:

<?xml version="1.0" encoding="utf-8"?>
<configuration>
  <location path="." inheritInChildApplications="false">
    <system.webServer>
      <handlers>
        <add name="aspNetCore"
             path="*"
             verb="*"
             modules="AspNetCoreModuleV2"
             resourceType="Unspecified" />
      </handlers>
      <aspNetCore processPath="dotnet"
                  arguments=".\Tender.Api.dll"
                  hostingModel="inprocess"
                  stdoutLogEnabled="false"
                  stdoutLogFile=".\logs\stdout">
        <environmentVariables>
          <environmentVariable name="ConnectionStrings__TenderDb"
                               value="Server=.;Database=TenderDb;Integrated Security=True;Trust Server Certificate=True;" />
          <environmentVariable name="Jwt__Secret"
                               value="ReplaceWithMinimum32CharSecretKeyHere!" />
          <environmentVariable name="Jwt__Issuer"
                               value="TenderApi" />
          <environmentVariable name="Jwt__Audience"
                               value="TenderApiClients" />
          <environmentVariable name="Jwt__ExpiryMinutes"
                               value="60" />
        </environmentVariables>
      </aspNetCore>
    </system.webServer>
  </location>
</configuration>

3) ### Find Deploy/Site/appsettings.json and Make it:

{
  "ConnectionStrings": {
    "TenderDb": "Server=.;Database=TenderDb;Integrated Security=True;Trust Server Certificate=True;"
  },
  "Jwt": {
    "Secret": "ReplaceWithMinimum32CharSecretKeyHere!",
    "Issuer": "TenderApi",
    "Audience": "TenderApiClients",
    "ExpiryMinutes": 60
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

4) ### Locate the script create_login.sql in Deploy/Scripts/:

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

5) ### Generate an idempotent migration script, in TenderManagement/Tender.Infrastructure folder, run:

dotnet ef migrations script --idempotent -o ../../Deploy/Scripts/schema.sql

6) ### Create the PowerShell installer (Setup_IIS.ps1) in Deploy/Scripts/:

<#====================================================================
  Tender Management API – Setup_IIS.ps1
  • Run elevated PowerShell.
  • Place this in Deploy\Scripts.
====================================================================#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

$deployRoot  = Split-Path $PSScriptRoot -Parent
$siteSource  = Join-Path $deployRoot "Site"
$siteDest    = "C:\inetpub\TenderApiSite"
$createLogin = Join-Path $PSScriptRoot "create_login.sql"
$schemaSql   = Join-Path $PSScriptRoot "schema.sql"

$siteName    = "TenderApi"
$appPoolName = "TenderApi"
$port        = 8090

# Verify sqlcmd
if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    throw "sqlcmd.exe not found. Install SQL CMD Utilities."
}
Import-Module WebAdministration

# Copy site
if (-not (Test-Path $siteDest)) { New-Item -ItemType Directory -Path $siteDest }
Copy-Item "$siteSource\*" $siteDest -Recurse -Force

# App Pool
if (-not (Test-Path "IIS:\AppPools\$appPoolName")) {
    New-WebAppPool -Name $appPoolName
    Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name managedRuntimeVersion -Value ""
}
Start-WebAppPool $appPoolName

# Site binding
if (-not (Test-Path "IIS:\Sites\$siteName")) {
    New-Website -Name $siteName -Port $port -PhysicalPath $siteDest -ApplicationPool $appPoolName
} else {
    Set-ItemProperty "IIS:\Sites\$siteName" -Name physicalPath -Value $siteDest
    Remove-WebBinding -Name $siteName -Protocol http -Port * -ErrorAction SilentlyContinue
    New-WebBinding -Name $siteName -Protocol http -Port $port -IPAddress *
}

# ACLs
icacls $siteDest /grant "IIS_IUSRS:(OI)(CI)M" /t

# SQL prompts
$sqlInstance = Read-Host "SQL Server instance (default '.')"
if ([string]::IsNullOrWhiteSpace($sqlInstance)) { $sqlInstance = "." }

$useIntegrated = Read-Host "Use Integrated Security? (Y/n)"
if ($useIntegrated -match '^[Yy]') {
    $authSwitch = "-E"
} else {
    $login    = Read-Host "SQL login"
    $password = Read-Host "SQL password" -AsSecureString
    $pwd      = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                  [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))
    $authSwitch = "-U $login -P $pwd"
}

# Run SQL scripts
Write-Host "`nCreating DB/login…"
sqlcmd -S $sqlInstance $authSwitch -d master -i $createLogin

Write-Host "`nApplying migrations…"
sqlcmd -S $sqlInstance $authSwitch -d TenderDb -i $schemaSql

# Recycle and finish
Restart-WebAppPool $appPoolName
Write-Host "`n✅ Deployment complete. Test: http://localhost:$port/api/health"

7) ### Now the Deploy Folder is ready!
