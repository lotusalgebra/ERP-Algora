-- Fix eCommerce Schema - Add missing columns
-- Run this script after CreateEcommerceTables.sql if you encounter missing column errors

SET QUOTED_IDENTIFIER ON;
GO

-- Add missing columns to WebCustomers
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebCustomers') AND name = 'AvatarUrl')
    ALTER TABLE WebCustomers ADD AvatarUrl NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebCustomers') AND name = 'DateOfBirth')
    ALTER TABLE WebCustomers ADD DateOfBirth DATE NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebCustomers') AND name = 'PasswordResetToken')
    ALTER TABLE WebCustomers ADD PasswordResetToken NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebCustomers') AND name = 'PasswordResetExpiry')
    ALTER TABLE WebCustomers ADD PasswordResetExpiry DATETIME2 NULL;

PRINT 'WebCustomers columns updated';
GO

-- Add missing columns to WebOrders
IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebOrders') AND name = 'CancelReason')
    ALTER TABLE WebOrders ADD CancelReason NVARCHAR(500) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebOrders') AND name = 'CancelledAt')
    ALTER TABLE WebOrders ADD CancelledAt DATETIME2 NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebOrders') AND name = 'Currency')
    ALTER TABLE WebOrders ADD Currency NVARCHAR(10) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebOrders') AND name = 'Notes')
    ALTER TABLE WebOrders ADD Notes NVARCHAR(1000) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebOrders') AND name = 'OrderDate')
    ALTER TABLE WebOrders ADD OrderDate DATETIME2 NOT NULL DEFAULT GETUTCDATE();

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebOrders') AND name = 'ShippingPhone')
    ALTER TABLE WebOrders ADD ShippingPhone NVARCHAR(50) NULL;

IF NOT EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('WebOrders') AND name = 'TaxRate')
    ALTER TABLE WebOrders ADD TaxRate DECIMAL(5,2) NULL;

PRINT 'WebOrders columns updated';
GO

PRINT 'eCommerce schema fix completed successfully';
GO
