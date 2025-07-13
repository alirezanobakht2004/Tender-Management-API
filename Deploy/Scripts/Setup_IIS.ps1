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

