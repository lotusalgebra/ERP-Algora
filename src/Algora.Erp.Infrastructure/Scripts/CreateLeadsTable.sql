-- Create Leads table for CRM functionality
-- Run this script against the tenant database (e.g., AlgoraErp_Dev)

SET QUOTED_IDENTIFIER ON;
GO

IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Leads')
BEGIN
    CREATE TABLE Leads (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        Company NVARCHAR(200) NULL,
        Email NVARCHAR(255) NULL,
        Phone NVARCHAR(50) NULL,
        Website NVARCHAR(500) NULL,

        Source INT NOT NULL DEFAULT 0, -- 0=Website, 1=Referral, 2=SocialMedia, etc.
        Status INT NOT NULL DEFAULT 0, -- 0=New, 1=Contacted, 2=Qualified, etc.
        Rating INT NOT NULL DEFAULT 0, -- 0=Cold, 1=Warm, 2=Hot

        EstimatedValue DECIMAL(18, 2) NULL,
        EstimatedCloseInDays INT NULL,

        Address NVARCHAR(500) NULL,
        City NVARCHAR(100) NULL,
        State NVARCHAR(100) NULL,
        Country NVARCHAR(100) NULL,
        PostalCode NVARCHAR(20) NULL,

        AssignedToId UNIQUEIDENTIFIER NULL,
        AssignedToName NVARCHAR(200) NULL,

        LastContactDate DATETIME2 NULL,
        NextFollowUpDate DATETIME2 NULL,

        Notes NVARCHAR(2000) NULL,
        Tags NVARCHAR(500) NULL,

        ConvertedCustomerId UNIQUEIDENTIFIER NULL,
        ConvertedAt DATETIME2 NULL,

        -- Audit fields
        IsDeleted BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER NULL,
        ModifiedAt DATETIME2 NULL,
        ModifiedBy UNIQUEIDENTIFIER NULL,
        DeletedAt DATETIME2 NULL,
        DeletedBy UNIQUEIDENTIFIER NULL
    );

    CREATE INDEX IX_Leads_Email ON Leads(Email) WHERE IsDeleted = 0;
    CREATE INDEX IX_Leads_Status ON Leads(Status) WHERE IsDeleted = 0;
    CREATE INDEX IX_Leads_Source ON Leads(Source) WHERE IsDeleted = 0;
    CREATE INDEX IX_Leads_Rating ON Leads(Rating) WHERE IsDeleted = 0;
    CREATE INDEX IX_Leads_NextFollowUpDate ON Leads(NextFollowUpDate) WHERE IsDeleted = 0;

    PRINT 'Created Leads table';
END
GO

PRINT 'Leads table setup complete!';
GO
