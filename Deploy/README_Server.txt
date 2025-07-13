# Tender Management API – Deployment Guide

This guide walks you—step by step—through turning the **Deploy** folder into a running API on IIS + SQL Server (works on Windows 10/11 or Windows Server). No developer tools required beyond what’s below.

---

## 1. Prerequisites (once per machine)
1. **Windows** (Desktop or Server) with IIS role + Management Console enabled.
2. **.NET 8 Hosting Bundle** installed. Download and run:
   https://builds.dotnet.microsoft.com/dotnet/aspnetcore/Runtime/8.0.18/dotnet-hosting-8.0.18-win.exe
3. **SQL Server** (Express, Developer, or Standard) installed and running locally.
4. **SQLCMD** utility available in your PATH (install SQL Server Command-Line Utilities if missing).

---

## 2. Unpack the Deploy folder
1. Copy or unzip the entire **Deploy** folder to `C:\Deploy`.
2. Under `C:\Deploy` confirm you see:
   - **Site** (with `web.config`, `appsettings.json`, `Tender.Api.dll`, etc.)
   - **Scripts** (with `create_login.sql`, `schema.sql`, `Setup_IIS.ps1`)
   - **README_Server.txt** (this guide)

---

## 3. Run the automated installer
1. Open **PowerShell as Administrator**.
2. Run:
   ```powershell
   cd C:\Deploy\Scripts
   .\Setup_IIS.ps1
   ```
3. When prompted:
   - **SQL Server instance**: press **Enter** for default (`.`).
   - **Use Integrated Security?** press **Y** (recommended) or **N** to supply a SQL login and password.

   The script will:
   - Copy **Site** → `C:\inetpub\TenderApiSite`
   - Create/update the **TenderApi** app-pool (No managed code)
   - Create/update the **TenderApi** site bound to port **8090**
   - Grant `IIS_IUSRS` Modify rights on that folder
   - Create the **TenderDb** database and login/user as chosen
   - Apply EF Core migrations idempotently (`schema.sql`)
   - Recycle the **TenderApi** app-pool

   On success you’ll see:
   ```
   ✅  Deployment complete. Test: http://localhost:8090/health
   ```

---

## 4. Verify the deployment
```powershell
curl.exe -i http://localhost:8090/health
```
- Should return **HTTP/1.1 200 OK** with body `OK`.

```powershell
curl.exe -i http://localhost:8090/api/tenders
```
- Should return **200 OK** with `[]` (if no data) or **401 Unauthorized**.

If you don’t see **500** errors, you’re live.

---

## 5. (Optional) Override connection & JWT settings
The package ships with defaults in **web.config** and **appsettings.json**. Only change if necessary:

1. Open `C:\inetpub\TenderApiSite\web.config` in Notepad.
2. In `<environmentVariables>`, update any of:
   ```xml
   <environmentVariable name="ConnectionStrings__TenderDb"
                        value="Server=<your-server>;Database=TenderDb;User Id=…;Password=…;Trust Server Certificate=True;" />
   <environmentVariable name="Jwt__Secret"   value="<your-32+-char-secret>" />
   <environmentVariable name="Jwt__Issuer"   value="TenderApi" />
   <environmentVariable name="Jwt__Audience" value="TenderApiClients" />
   <environmentVariable name="Jwt__ExpiryMinutes" value="60" />
   ```
3. Save the file and recycle the pool:
   ```powershell
   Restart-WebAppPool -Name TenderApi
   ```

If you don’t need to change anything, skip this step entirely.

---

## 6. Firewall & HTTPS (optional)
- **Firewall:** open inbound port **8090** in Windows Defender Firewall.
- **HTTPS:** to add SSL later, bind a certificate in IIS Manager → **Bindings → Add → https**.

---

## Appendix: Common Issues & Solutions
- **Port 8090 already in use**
  1. Edit the first line of `Setup_IIS.ps1`, change `$port = 8090` to a free port (e.g. `8081`).
  2. Rerun `.\Setup_IIS.ps1` and test with the new port.

- **`sqlcmd.exe` not found**
  Install Microsoft **SQL Server Command-Line Utilities**, or adjust your PATH.

- **Database authentication failures**
  - *Integrated:* ensure `IIS APPPOOL\TenderApi` exists as a SQL login.
  - *SQL Auth:* verify logins are enabled and credentials are correct.

- **500 Errors on `/health` or API**
  1. Ensure `<aspNetCore stdoutLogEnabled="true">` in `web.config`.
  2. Make sure `C:\inetpub\TenderApiSite\logs` exists:
     ```powershell
     New-Item -ItemType Directory -Path C:\inetpub\TenderApiSite\logs -Force
     icacls "C:\inetpub\TenderApiSite\logs" /grant "IIS_IUSRS:(OI)(CI)M" /t
     Restart-WebAppPool -Name TenderApi
     ```
  3. Check `stdout_*.log` or Event Viewer → Application.

- **404 on `/health`**
  Call `http://<host>:<port>/health` (not `/api/health`). 
  To use `/api/health`, update controller route, republish, redeploy.

- **File permissions errors**
  In Admin PowerShell:
  ```powershell
  icacls "C:\inetpub\TenderApiSite" /grant "IIS_IUSRS:(OI)(CI)M" /t
  Restart-WebAppPool -Name TenderApi
  ```

---

Done!