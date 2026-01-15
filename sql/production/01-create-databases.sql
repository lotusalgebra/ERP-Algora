-- =============================================
-- Algora ERP Production Database Setup
-- Run this script on your SQL Server to create
-- the required databases for production
-- =============================================

-- Create Master Database (Multi-tenant management)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AlgoraErp_Master')
BEGIN
    CREATE DATABASE [AlgoraErp_Master]
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Created database: AlgoraErp_Master';
END
GO

-- Create Admin Database (Admin portal)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AlgoraErpAdmin')
BEGIN
    CREATE DATABASE [AlgoraErpAdmin]
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Created database: AlgoraErpAdmin';
END
GO

-- Create Default Tenant Database (First tenant)
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'AlgoraErp_Tenant1')
BEGIN
    CREATE DATABASE [AlgoraErp_Tenant1]
    COLLATE SQL_Latin1_General_CP1_CI_AS;
    PRINT 'Created database: AlgoraErp_Tenant1';
END
GO

-- =============================================
-- Create Application Login
-- Replace 'YourStrongPassword!' with actual password
-- =============================================

USE [master]
GO

IF NOT EXISTS (SELECT name FROM sys.server_principals WHERE name = 'AlgoraErpApp')
BEGIN
    CREATE LOGIN [AlgoraErpApp] WITH PASSWORD = N'YourStrongPassword!',
        DEFAULT_DATABASE = [AlgoraErp_Master],
        CHECK_EXPIRATION = OFF,
        CHECK_POLICY = ON;
    PRINT 'Created login: AlgoraErpApp';
END
GO

-- Grant permissions on Master database
USE [AlgoraErp_Master]
GO
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'AlgoraErpApp')
BEGIN
    CREATE USER [AlgoraErpApp] FOR LOGIN [AlgoraErpApp];
    ALTER ROLE [db_owner] ADD MEMBER [AlgoraErpApp];
    PRINT 'Granted db_owner on AlgoraErp_Master';
END
GO

-- Grant permissions on Admin database
USE [AlgoraErpAdmin]
GO
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'AlgoraErpApp')
BEGIN
    CREATE USER [AlgoraErpApp] FOR LOGIN [AlgoraErpApp];
    ALTER ROLE [db_owner] ADD MEMBER [AlgoraErpApp];
    PRINT 'Granted db_owner on AlgoraErpAdmin';
END
GO

-- Grant permissions on Tenant database
USE [AlgoraErp_Tenant1]
GO
IF NOT EXISTS (SELECT name FROM sys.database_principals WHERE name = 'AlgoraErpApp')
BEGIN
    CREATE USER [AlgoraErpApp] FOR LOGIN [AlgoraErpApp];
    ALTER ROLE [db_owner] ADD MEMBER [AlgoraErpApp];
    PRINT 'Granted db_owner on AlgoraErp_Tenant1';
END
GO

PRINT '';
PRINT '=============================================';
PRINT 'Database setup complete!';
PRINT '';
PRINT 'Connection string format:';
PRINT 'Server=YOUR_SERVER;Database=DATABASE_NAME;User Id=AlgoraErpApp;Password=YourStrongPassword!;TrustServerCertificate=True;Encrypt=True;';
PRINT '';
PRINT 'IMPORTANT: Change the password above before running!';
PRINT '=============================================';
GO
