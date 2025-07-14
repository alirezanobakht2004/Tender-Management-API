# Tender Management API

## Overview
The Tender Management API is a .NET backend service for managing tenders, bids, vendors, and user authentication. It provides RESTful endpoints for creating, reading, updating, and deleting resources, with secure JWT-based authentication and role-based authorization.

**Total Development Time**: ~20–25 hours

## Solution Structure
All code and assets are organized under the **TenderManagement** solution:

```
TenderManagement/
├─ Tender.Api/              # ASP.NET Core Web API project
├─ Tender.Application/      # Application layer (business logic, MediatR handlers)
├─ Tender.Domain/           # Domain entities and value objects
├─ Tender.Infrastructure/   # EF Core persistence, Dapper queries
├─ Tender.Tests/            # xUnit integration and API tests
TenderManagement.sln        # Solution file
```

Additionally:

- **Postman/**
  - `Tender API.postman_environment.json`
  - `Tender Management API.postman_collection.json`
  - `PostmanCollection-README.md` (guide for secure routes)

- **Database Backup/**
  - `TenderDb.bak` (optional SQL Server backup)

- **Deploy/**
  - `Tender_Management_API_Deployment_Guide.md` (deployment instructions for IIS)
  - **Site/** (published API files)
  - **Scripts/** (SQL and PowerShell scripts)

- **Document/**
  - Developer commands to make project deployable (`Developer Commands to make the Project Deployable.txt`)
  - `Project_Structure.pdf`
  - `Tender Management API Backend – Technical Specification and Development Guide.pdf`

## Prerequisites
- .NET SDK 8.0 (or higher)
- SQL Server (any edition)
- IIS (Windows)
- PowerShell execution policy set to allow script execution
- Optional: Postman for API testing

## Running Tests
After deployment (and once `TenderDb` is created with its tables):

1. Open a command prompt in the **TenderManagement/** folder.
2. Set the connection string environment variable:
   ```cmd
   set ConnectionStrings__TenderDb=Server=.;Database=TenderDb;Integrated Security=True;TrustServerCertificate=True;
   ```
3. Run the xUnit tests:
   ```cmd
   dotnet test Tender.Tests\Tender.Tests.csproj --no-build
   ```

_Postman_ provides a collection of happy-path API tests; see **Postman/PostmanCollection-README.md** for secure route guidance.

## Deployment
Full IIS‑based deployment steps are in **Deploy/Tender_Management_API_Deployment_Guide.md**. Follow that guide to:

1. Copy the **Deploy/** folder to the server.
2. Run **Deploy/Scripts/Setup_IIS.ps1** (elevated PowerShell).
3. Ensure the application pool, site, and database are created.

## Documentation
- **Developer Commands to make the Project Deployable.txt**
- **Project_Structure.pdf** (overview diagram)
- **Tender Management API Backend – Technical Specification and Development Guide.pdf**

## Support
For questions or issues, please contact:

Alireza Nobakht  
📧 alireza.nobakht2004@gmail.com
