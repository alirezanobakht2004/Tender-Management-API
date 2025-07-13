<#====================================================================
  Tender Management API – Setup_IIS.ps1
  • Run in an elevated PowerShell window.
  • Assumes this script sits in Deploy\Scripts.
====================================================================#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# -------------------- paths & constants ------------------------------------
$deployRoot   = (Split-Path $PSScriptRoot -Parent)
$siteSource   = Join-Path $deployRoot "Site"
$siteDest     = "C:\inetpub\TenderApiSite"
$createLogin  = Join-Path $PSScriptRoot "create_login.sql"
$schemaSql    = Join-Path $PSScriptRoot "schema.sql"

$siteName     = "TenderApi"
$appPoolName  = "TenderApi"
$port         = 8090                    # change if needed

# -------------------- verify prerequisites ---------------------------------
if (-not (Get-Command sqlcmd -ErrorAction SilentlyContinue)) {
    Write-Error "sqlcmd.exe not found in PATH. Install SQL Command-Line Utilities first."
    exit 1
}
Import-Module WebAdministration

# -------------------- copy web site ----------------------------------------
if (-not (Test-Path $siteDest)) { New-Item -ItemType Directory -Path $siteDest | Out-Null }
Copy-Item "$siteSource\*" $siteDest -Recurse -Force

# -------------------- IIS: app pool ----------------------------------------
if (-not (Test-Path "IIS:\AppPools\$appPoolName")) {
    New-WebAppPool -Name $appPoolName | Out-Null
    Set-ItemProperty "IIS:\AppPools\$appPoolName" -Name managedRuntimeVersion -Value ""
}
Start-WebAppPool $appPoolName

# -------------------- IIS: site --------------------------------------------
if (-not (Test-Path "IIS:\Sites\$siteName")) {
    New-Website -Name $siteName -Port $port -PhysicalPath $siteDest -ApplicationPool $appPoolName | Out-Null
} else {
    Set-ItemProperty "IIS:\Sites\$siteName" -Name physicalPath -Value $siteDest
}

# -------------------- folder ACL -------------------------------------------
icacls $siteDest /grant "IIS_IUSRS:(OI)(CI)M" /t | Out-Null

# -------------------- SQL auth choice --------------------------------------
Write-Host ""
Write-Host "== SQL connection parameters =="
$sqlInstance = Read-Host "SQL Server instance (default '.')"
if ([string]::IsNullOrWhiteSpace($sqlInstance)) { $sqlInstance = "." }

$useIntegrated = Read-Host "Use Integrated Security? (Y/n)"
if ($useIntegrated -eq "" -or $useIntegrated -match '^[Yy]') {
    $authSwitch = "-E"
} else {
    $login    = Read-Host "SQL login name"
    $password = Read-Host "SQL login password (hidden)" -AsSecureString
    $plainPwd = [Runtime.InteropServices.Marshal]::PtrToStringAuto(
                  [Runtime.InteropServices.Marshal]::SecureStringToBSTR($password))
    $authSwitch = "-U $login -P `"$plainPwd`""
}

# -------------------- execute SQL scripts ----------------------------------
Write-Host "`n>> Creating DB / login …"
sqlcmd -S $sqlInstance $authSwitch -d master -i $createLogin

Write-Host "`n>> Applying EF migrations …"
sqlcmd -S $sqlInstance $authSwitch -d master -i $schemaSql

# -------------------- recycle pool & done ----------------------------------
Restart-WebAppPool $appPoolName
Write-Host "`nDeployment finished. Test the API at http://localhost:$port/health"
