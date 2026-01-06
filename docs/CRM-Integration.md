# CRM Integration Documentation

Algora ERP supports bi-directional synchronization with **Salesforce** and **Microsoft Dynamics 365 CRM**. This document covers setup, configuration, and usage of these integrations.

---

## Table of Contents

1. [Overview](#overview)
2. [Supported Entities](#supported-entities)
3. [Salesforce Integration](#salesforce-integration)
   - [Prerequisites](#salesforce-prerequisites)
   - [Setup Guide](#salesforce-setup-guide)
   - [Configuration](#salesforce-configuration)
4. [Dynamics 365 Integration](#dynamics-365-integration)
   - [Prerequisites](#dynamics-365-prerequisites)
   - [Setup Guide](#dynamics-365-setup-guide)
   - [Configuration](#dynamics-365-configuration)
5. [Database Setup](#database-setup)
6. [Admin UI](#admin-ui)
7. [Sync Operations](#sync-operations)
8. [Troubleshooting](#troubleshooting)
9. [API Reference](#api-reference)

---

## Overview

The CRM integration module (`Algora.Erp.Integrations`) provides:

- **Bi-directional sync** of contacts, leads, opportunities, and accounts
- **Scheduled background sync** at configurable intervals
- **Manual sync** triggers from the admin UI
- **Sync history** and error logging
- **Multi-tenant support** with per-tenant credentials

### Architecture

```
┌─────────────────────────────────────────────────────────────┐
│                      Algora ERP                              │
├─────────────────────────────────────────────────────────────┤
│  Algora.Erp.Integrations                                     │
│  ├── Common (Interfaces, Models, Settings)                   │
│  ├── Salesforce (Auth, Client, Services)                     │
│  ├── Dynamics365 (Auth, Client, Services)                    │
│  └── BackgroundServices (CrmSyncBackgroundService)           │
├─────────────────────────────────────────────────────────────┤
│                    ↓              ↓                          │
│            Salesforce API    Dynamics 365 API                │
│            (REST v58.0)      (Dataverse v9.2)                │
└─────────────────────────────────────────────────────────────┘
```

---

## Supported Entities

| ERP Entity | Salesforce Object | Dynamics 365 Entity |
|------------|-------------------|---------------------|
| Customer   | Contact           | contact             |
| Sales Lead | Lead              | lead                |
| Quotation  | Opportunity       | opportunity         |
| Company    | Account           | account             |

### Field Mappings

#### Contact/Customer
| ERP Field    | Salesforce Field | Dynamics 365 Field    |
|--------------|------------------|-----------------------|
| FirstName    | FirstName        | firstname             |
| LastName     | LastName         | lastname              |
| Email        | Email            | emailaddress1         |
| Phone        | Phone            | telephone1            |
| MobilePhone  | MobilePhone      | mobilephone           |
| Title        | Title            | jobtitle              |
| Department   | Department       | department            |

#### Lead
| ERP Field      | Salesforce Field   | Dynamics 365 Field    |
|----------------|--------------------|-----------------------|
| FirstName      | FirstName          | firstname             |
| LastName       | LastName           | lastname              |
| Company        | Company            | companyname           |
| Email          | Email              | emailaddress1         |
| Status         | Status             | statuscode            |
| LeadSource     | LeadSource         | leadsourcecode        |
| AnnualRevenue  | AnnualRevenue      | revenue               |

#### Opportunity
| ERP Field    | Salesforce Field | Dynamics 365 Field    |
|--------------|------------------|-----------------------|
| Name         | Name             | name                  |
| Amount       | Amount           | totalamount           |
| Stage        | StageName        | salesstagecode        |
| Probability  | Probability      | closeprobability      |
| CloseDate    | CloseDate        | estimatedclosedate    |

#### Account
| ERP Field        | Salesforce Field   | Dynamics 365 Field    |
|------------------|--------------------|-----------------------|
| Name             | Name               | name                  |
| AccountNumber    | AccountNumber      | accountnumber         |
| Industry         | Industry           | industrycode          |
| Phone            | Phone              | telephone1            |
| Website          | Website            | websiteurl            |
| AnnualRevenue    | AnnualRevenue      | revenue               |

---

## Salesforce Integration

### Salesforce Prerequisites

1. Salesforce account with API access (Enterprise, Unlimited, or Developer edition)
2. Connected App with OAuth enabled
3. API user credentials
4. Security Token

### Salesforce Setup Guide

#### Step 1: Create a Connected App

1. Log in to Salesforce Setup
2. Navigate to: **Setup → Apps → App Manager**
3. Click **New Connected App**
4. Fill in the details:
   - **Connected App Name**: Algora ERP Integration
   - **API Name**: Algora_ERP_Integration
   - **Contact Email**: your-email@company.com
5. Enable OAuth Settings:
   - **Enable OAuth Settings**: ✓
   - **Callback URL**: `https://localhost` (not used for password flow)
   - **Selected OAuth Scopes**:
     - Access and manage your data (api)
     - Perform requests on your behalf at any time (refresh_token, offline_access)
     - Full access (full)
6. Click **Save** and wait 2-10 minutes for activation

#### Step 2: Get Consumer Key and Secret

1. Go to **Setup → Apps → App Manager**
2. Find your Connected App and click **View**
3. Click **Manage Consumer Details**
4. Verify your identity
5. Copy the **Consumer Key** and **Consumer Secret**

#### Step 3: Get Security Token

1. Go to **Settings → My Personal Information → Reset My Security Token**
2. Click **Reset Security Token**
3. Check your email for the new token

#### Step 4: Create Integration User (Recommended)

1. Create a dedicated user for API integration
2. Assign appropriate profile with API access
3. Note down the username and password

### Salesforce Configuration

Add the following to `appsettings.json`:

```json
{
  "CrmIntegrations": {
    "Salesforce": {
      "Enabled": true,
      "ClientId": "your-consumer-key",
      "ClientSecret": "your-consumer-secret",
      "Username": "integration-user@company.com",
      "Password": "user-password",
      "SecurityToken": "security-token-from-email",
      "InstanceUrl": "https://login.salesforce.com",
      "ApiVersion": "v58.0",
      "SyncIntervalMinutes": 30
    }
  }
}
```

| Setting | Description |
|---------|-------------|
| `Enabled` | Set to `true` to enable the integration |
| `ClientId` | Consumer Key from Connected App |
| `ClientSecret` | Consumer Secret from Connected App |
| `Username` | Salesforce username |
| `Password` | Salesforce password |
| `SecurityToken` | Security token (appended to password) |
| `InstanceUrl` | `https://login.salesforce.com` for production, `https://test.salesforce.com` for sandbox |
| `ApiVersion` | Salesforce API version (default: v58.0) |
| `SyncIntervalMinutes` | How often to run automatic sync |

---

## Dynamics 365 Integration

### Dynamics 365 Prerequisites

1. Microsoft Dynamics 365 environment
2. Azure AD tenant with admin access
3. App registration in Azure AD
4. Application user in Dynamics 365

### Dynamics 365 Setup Guide

#### Step 1: Register Application in Azure AD

1. Go to [Azure Portal](https://portal.azure.com)
2. Navigate to: **Azure Active Directory → App registrations**
3. Click **New registration**
4. Fill in the details:
   - **Name**: Algora ERP Integration
   - **Supported account types**: Single tenant
   - **Redirect URI**: Leave blank
5. Click **Register**
6. Copy the **Application (client) ID** and **Directory (tenant) ID**

#### Step 2: Create Client Secret

1. In your App registration, go to **Certificates & secrets**
2. Click **New client secret**
3. Add a description and select expiry period
4. Click **Add**
5. **Copy the secret value immediately** (it won't be shown again)

#### Step 3: Add API Permissions

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Dynamics CRM**
4. Select **Delegated permissions**
5. Check **user_impersonation**
6. Click **Add permissions**
7. Click **Grant admin consent for [Your Organization]**

#### Step 4: Create Application User in Dynamics 365

1. Go to your Dynamics 365 environment
2. Navigate to: **Settings → Security → Users**
3. Change view to **Application Users**
4. Click **New**
5. Select **Application User** form
6. Fill in:
   - **Application ID**: The Client ID from Azure AD
   - **Full Name**: Algora ERP Integration
   - **Primary Email**: integration@company.com
7. Save the user
8. Assign security roles (e.g., System Administrator or custom role with required permissions)

### Dynamics 365 Configuration

Add the following to `appsettings.json`:

```json
{
  "CrmIntegrations": {
    "Dynamics365": {
      "Enabled": true,
      "TenantId": "your-azure-tenant-id",
      "ClientId": "your-application-client-id",
      "ClientSecret": "your-client-secret",
      "InstanceUrl": "https://yourorg.crm.dynamics.com",
      "ApiVersion": "v9.2",
      "SyncIntervalMinutes": 30
    }
  }
}
```

| Setting | Description |
|---------|-------------|
| `Enabled` | Set to `true` to enable the integration |
| `TenantId` | Azure AD Directory (tenant) ID |
| `ClientId` | Azure AD Application (client) ID |
| `ClientSecret` | Client secret from Azure AD |
| `InstanceUrl` | Your Dynamics 365 environment URL |
| `ApiVersion` | Dataverse API version (default: v9.2) |
| `SyncIntervalMinutes` | How often to run automatic sync |

---

## Database Setup

Run the SQL migration script to create required tables:

```bash
sqlcmd -S (localdb)\mssqllocaldb -d AlgoraErp_TenantDb -i src/Algora.Erp.Integrations/Scripts/CreateCrmTables.sql
```

### Tables Created

| Table | Purpose |
|-------|---------|
| `CrmIntegrationMappings` | Maps ERP entities to CRM entities |
| `CrmSyncLogs` | Stores sync history and results |
| `TenantCrmCredentials` | Stores encrypted CRM credentials per tenant |
| `CrmFieldMappings` | Custom field mappings (optional) |
| `CrmSyncQueue` | Queue for async sync operations |

### Schema Details

```sql
-- CrmIntegrationMappings
CREATE TABLE CrmIntegrationMappings (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    CrmType NVARCHAR(50) NOT NULL,        -- 'Salesforce' or 'Dynamics365'
    EntityType NVARCHAR(100) NOT NULL,    -- 'Contact', 'Lead', etc.
    ErpEntityId UNIQUEIDENTIFIER NOT NULL,
    CrmEntityId NVARCHAR(100) NOT NULL,
    LastSyncedAt DATETIME2 NOT NULL,
    SyncStatus NVARCHAR(50) NOT NULL,
    LastSyncError NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL
);

-- CrmSyncLogs
CREATE TABLE CrmSyncLogs (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    TenantId UNIQUEIDENTIFIER NOT NULL,
    CrmType NVARCHAR(50) NOT NULL,
    EntityType NVARCHAR(100) NULL,
    Direction NVARCHAR(20) NOT NULL,      -- 'ToErp', 'ToCrm', 'Bidirectional'
    RecordsProcessed INT NOT NULL,
    RecordsSucceeded INT NOT NULL,
    RecordsFailed INT NOT NULL,
    StartedAt DATETIME2 NOT NULL,
    CompletedAt DATETIME2 NULL,
    ErrorMessage NVARCHAR(MAX) NULL,
    CreatedAt DATETIME2 NOT NULL
);
```

---

## Admin UI

Access the CRM integration settings at:

### Integration Dashboard
**URL**: `/Admin/Integrations`

- View connection status for both CRMs
- See sync statistics
- Trigger manual sync
- View recent sync history

### Salesforce Settings
**URL**: `/Admin/Integrations/Salesforce`

- Configure Salesforce credentials
- Test connection
- Select entities to sync
- Set sync interval
- Trigger manual sync

### Dynamics 365 Settings
**URL**: `/Admin/Integrations/Dynamics365`

- Configure Azure AD credentials
- Test connection
- Select entities to sync
- Set sync interval
- Trigger manual sync

### Sync History
**URL**: `/Admin/Integrations/SyncHistory`

- View all sync operations
- Filter by CRM, entity type, status, date
- View error details
- Export sync logs

---

## Sync Operations

### Automatic Sync

When enabled, the `CrmSyncBackgroundService` runs at the configured interval:

1. Checks which CRMs are enabled
2. For each enabled CRM, runs `FullSyncAsync()`
3. Syncs entities in order: Accounts → Contacts → Leads → Opportunities
4. Logs results to `CrmSyncLogs` table

### Manual Sync

Trigger sync from the Admin UI or programmatically:

```csharp
// Inject the sync service
public class MyController : Controller
{
    private readonly IEnumerable<ICrmSyncService> _syncServices;

    public async Task<IActionResult> SyncSalesforce()
    {
        var service = _syncServices.FirstOrDefault(s => s.CrmType == "Salesforce");
        var result = await service.FullSyncAsync();
        return Ok(result);
    }
}
```

### Sync Direction

| Direction | Description |
|-----------|-------------|
| `ToErp` | Pull data from CRM into ERP |
| `ToCrm` | Push data from ERP to CRM |
| `Bidirectional` | Sync both directions (default) |

---

## Troubleshooting

### Common Errors

#### Salesforce: "INVALID_LOGIN"
- Verify username and password
- Check that security token is appended to password
- Ensure user has API access
- For sandbox, use `https://test.salesforce.com`

#### Salesforce: "INVALID_CLIENT"
- Verify Consumer Key and Secret
- Ensure Connected App is activated (wait 2-10 minutes after creation)

#### Dynamics 365: "AADSTS7000215"
- Client secret may have expired
- Create a new secret in Azure AD

#### Dynamics 365: "403 Forbidden"
- Application user doesn't have required security roles
- Grant appropriate permissions in Dynamics 365

#### Sync Fails with Timeout
- Increase timeout in `appsettings.json`
- Reduce batch size
- Check network connectivity

### Logging

Enable detailed logging in `appsettings.json`:

```json
{
  "Serilog": {
    "MinimumLevel": {
      "Override": {
        "Algora.Erp.Integrations": "Debug"
      }
    }
  }
}
```

### Testing Connection

Use the Admin UI "Test Connection" button or call programmatically:

```csharp
var client = serviceProvider.GetService<SalesforceClient>();
bool isConnected = await client.TestConnectionAsync();
```

---

## API Reference

### ICrmClient Interface

```csharp
public interface ICrmClient
{
    string CrmType { get; }
    Task<bool> TestConnectionAsync(CancellationToken ct = default);
    Task<T?> GetByIdAsync<T>(string id, CancellationToken ct = default);
    Task<List<T>> QueryAsync<T>(string query, CancellationToken ct = default);
    Task<string> CreateAsync<T>(T entity, CancellationToken ct = default);
    Task UpdateAsync<T>(string id, T entity, CancellationToken ct = default);
    Task DeleteAsync(string id, string entityType, CancellationToken ct = default);
    Task<List<T>> GetModifiedSinceAsync<T>(DateTime since, CancellationToken ct = default);
}
```

### ICrmSyncService Interface

```csharp
public interface ICrmSyncService
{
    string CrmType { get; }
    Task<SyncResult> SyncContactsAsync(SyncDirection direction, CancellationToken ct = default);
    Task<SyncResult> SyncLeadsAsync(SyncDirection direction, CancellationToken ct = default);
    Task<SyncResult> SyncOpportunitiesAsync(SyncDirection direction, CancellationToken ct = default);
    Task<SyncResult> SyncAccountsAsync(SyncDirection direction, CancellationToken ct = default);
    Task<SyncResult> FullSyncAsync(CancellationToken ct = default);
}
```

### SyncResult Model

```csharp
public class SyncResult
{
    public string CrmType { get; set; }
    public string EntityType { get; set; }
    public SyncDirection Direction { get; set; }
    public int RecordsProcessed { get; set; }
    public int RecordsCreated { get; set; }
    public int RecordsUpdated { get; set; }
    public int RecordsFailed { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; set; }
    public List<SyncError> Errors { get; set; }
}
```

---

## Security Considerations

1. **Store credentials securely**: Use Azure Key Vault or similar for production
2. **Use dedicated integration users**: Don't use admin accounts for API access
3. **Rotate secrets regularly**: Set reminders for client secret expiration
4. **Limit permissions**: Grant only necessary permissions to integration users
5. **Enable audit logging**: Track all sync operations for compliance
6. **Use HTTPS only**: All API calls use TLS encryption

---

## Support

For issues or questions:
- Check the [Troubleshooting](#troubleshooting) section
- Review sync logs at `/Admin/Integrations/SyncHistory`
- Contact support with error details and sync log ID
