
# Tender Management API – Postman Collection

## Overview

This Postman collection demonstrates the **best-case (happy-path) scenarios** for all endpoints required by the Tender Management API.  
- **All endpoints** listed in the project brief are covered.
- **Environment variables** (base URL, tokens, IDs) are pre-configured for easy setup.
- **Sample requests and responses** are provided for each main API flow.

**Note:**  
All **negative and edge scenarios** (e.g., invalid input, unauthorized, forbidden, not found) are robustly handled in the backend API. You can manually test these by changing request payloads/headers, or run the provided xUnit project (`dotnet test`) to verify backend validation and error handling.

---

## Getting Started

### 1. Import the Collection and Environment

- Import `Tender Management API.postman_collection.json` and `Tender API.postman_environment.json` into Postman.
- Set the active environment to **Tender API**.

### 2. Set Authorization

- The collection is secured using **JWT Bearer Authentication**.
- Authorization for all requests is set to **Bearer Token**, using the `adminUserToken` environment variable.
- Auth **type** is set to **“Inherit from parent”** for requests.

---

## Authentication Workflow

1. **Register as Admin (if not registered)**
    - Run `Register As Admin` (POST `/api/auth/register`).
2. **Login as Admin**
    - Run `Login As Admin` (POST `/api/auth/login`).
    - The JWT token is automatically saved to `adminUserToken` and used for all secured requests.

**Important:**  
- Only authenticated users with the **Admin** role can access routes such as creating, updating, or deleting tenders and approving/rejecting bids.
- If you log out or reset the token, rerun the login request to refresh your token.

---

## Using the Secured Routes

- Ensure you are logged in as an **Admin**; `adminUserToken` will be set automatically.
- All secured endpoints inherit this token from the collection’s Authorization settings.
- To test endpoints as a **Vendor**, adjust the token variable accordingly after logging in as a vendor.

---

## Negative/Edge Scenarios

- This collection demonstrates **best-case flows only**.
- **To test error scenarios** (e.g., missing fields, unauthorized access, forbidden actions):
    - Modify the request body, headers, or token manually in Postman.
    - Or, run the xUnit test project:  
      ```sh
      dotnet test
      ```
    - The backend will return appropriate RFC-7807 `ProblemDetails` responses for all error conditions.

---
