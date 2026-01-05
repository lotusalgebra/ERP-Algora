SET QUOTED_IDENTIFIER ON
GO

-- =============================================
-- PROJECT MANAGEMENT TABLES
-- Run this script in the tenant database
-- =============================================

-- Projects
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='Projects' AND xtype='U')
BEGIN
    CREATE TABLE Projects (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProjectCode NVARCHAR(20) NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(1000),
        CustomerId UNIQUEIDENTIFIER,
        ProjectManagerId UNIQUEIDENTIFIER,
        Status INT NOT NULL DEFAULT 0, -- 0=Planning, 1=Active, 2=OnHold, 3=Completed, 4=Cancelled
        Priority INT NOT NULL DEFAULT 1, -- 0=Low, 1=Normal, 2=High, 3=Critical
        StartDate DATETIME2 NOT NULL,
        EndDate DATETIME2,
        ActualStartDate DATETIME2,
        ActualEndDate DATETIME2,
        BudgetAmount DECIMAL(18,2) DEFAULT 0,
        ActualCost DECIMAL(18,2) DEFAULT 0,
        Progress DECIMAL(5,2) DEFAULT 0,
        IsActive BIT DEFAULT 1,
        Notes NVARCHAR(1000),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_Projects_Customer FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE SET NULL,
        CONSTRAINT FK_Projects_ProjectManager FOREIGN KEY (ProjectManagerId) REFERENCES Employees(Id) ON DELETE SET NULL
    );

    CREATE UNIQUE INDEX IX_Projects_ProjectCode ON Projects(ProjectCode) WHERE IsDeleted = 0;
    CREATE INDEX IX_Projects_Status ON Projects(Status) WHERE IsDeleted = 0;
END
GO

-- Project Tasks
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProjectTasks' AND xtype='U')
BEGIN
    CREATE TABLE ProjectTasks (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProjectId UNIQUEIDENTIFIER NOT NULL,
        ParentTaskId UNIQUEIDENTIFIER,
        AssigneeId UNIQUEIDENTIFIER,
        TaskNumber NVARCHAR(20) NOT NULL,
        Title NVARCHAR(200) NOT NULL,
        Description NVARCHAR(2000),
        Status INT NOT NULL DEFAULT 0, -- 0=Todo, 1=InProgress, 2=InReview, 3=Completed, 4=Cancelled
        Priority INT NOT NULL DEFAULT 1, -- 0=Low, 1=Normal, 2=High, 3=Urgent
        DueDate DATETIME2,
        StartedAt DATETIME2,
        CompletedAt DATETIME2,
        EstimatedHours DECIMAL(10,2) DEFAULT 0,
        ActualHours DECIMAL(10,2) DEFAULT 0,
        SortOrder INT DEFAULT 0,
        Notes NVARCHAR(1000),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_ProjectTasks_Project FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProjectTasks_ParentTask FOREIGN KEY (ParentTaskId) REFERENCES ProjectTasks(Id) ON DELETE NO ACTION,
        CONSTRAINT FK_ProjectTasks_Assignee FOREIGN KEY (AssigneeId) REFERENCES Employees(Id) ON DELETE SET NULL
    );

    CREATE INDEX IX_ProjectTasks_ProjectId ON ProjectTasks(ProjectId) WHERE IsDeleted = 0;
    CREATE INDEX IX_ProjectTasks_AssigneeId ON ProjectTasks(AssigneeId) WHERE IsDeleted = 0;
END
GO

-- Project Members
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProjectMembers' AND xtype='U')
BEGIN
    CREATE TABLE ProjectMembers (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProjectId UNIQUEIDENTIFIER NOT NULL,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        Role INT NOT NULL DEFAULT 0, -- 0=Member, 1=Lead, 2=Manager, 3=Stakeholder
        HourlyRate DECIMAL(18,2) DEFAULT 0,
        JoinedAt DATETIME2 DEFAULT GETUTCDATE(),
        LeftAt DATETIME2,
        IsActive BIT DEFAULT 1,
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_ProjectMembers_Project FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_ProjectMembers_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
    );

    CREATE UNIQUE INDEX IX_ProjectMembers_ProjectEmployee ON ProjectMembers(ProjectId, EmployeeId) WHERE IsDeleted = 0;
END
GO

-- Time Entries
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='TimeEntries' AND xtype='U')
BEGIN
    CREATE TABLE TimeEntries (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProjectId UNIQUEIDENTIFIER NOT NULL,
        TaskId UNIQUEIDENTIFIER,
        EmployeeId UNIQUEIDENTIFIER NOT NULL,
        Date DATE NOT NULL,
        StartTime DATETIME2,
        EndTime DATETIME2,
        Hours DECIMAL(5,2) NOT NULL,
        Description NVARCHAR(500),
        IsBillable BIT DEFAULT 1,
        Status INT NOT NULL DEFAULT 0, -- 0=Draft, 1=Submitted, 2=Approved, 3=Rejected
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_TimeEntries_Project FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE,
        CONSTRAINT FK_TimeEntries_Task FOREIGN KEY (TaskId) REFERENCES ProjectTasks(Id) ON DELETE SET NULL,
        CONSTRAINT FK_TimeEntries_Employee FOREIGN KEY (EmployeeId) REFERENCES Employees(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_TimeEntries_ProjectId ON TimeEntries(ProjectId) WHERE IsDeleted = 0;
    CREATE INDEX IX_TimeEntries_EmployeeId ON TimeEntries(EmployeeId) WHERE IsDeleted = 0;
    CREATE INDEX IX_TimeEntries_Date ON TimeEntries(Date) WHERE IsDeleted = 0;
END
GO

-- Project Milestones
IF NOT EXISTS (SELECT * FROM sysobjects WHERE name='ProjectMilestones' AND xtype='U')
BEGIN
    CREATE TABLE ProjectMilestones (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        ProjectId UNIQUEIDENTIFIER NOT NULL,
        Name NVARCHAR(200) NOT NULL,
        Description NVARCHAR(500),
        DueDate DATETIME2 NOT NULL,
        CompletedAt DATETIME2,
        Status INT NOT NULL DEFAULT 0, -- 0=Pending, 1=InProgress, 2=Completed, 3=Missed
        SortOrder INT DEFAULT 0,
        Notes NVARCHAR(500),
        CreatedAt DATETIME2 DEFAULT GETUTCDATE(),
        CreatedBy UNIQUEIDENTIFIER,
        ModifiedAt DATETIME2,
        ModifiedBy UNIQUEIDENTIFIER,
        IsDeleted BIT DEFAULT 0,
        DeletedAt DATETIME2,
        DeletedBy UNIQUEIDENTIFIER,
        CONSTRAINT FK_ProjectMilestones_Project FOREIGN KEY (ProjectId) REFERENCES Projects(Id) ON DELETE CASCADE
    );

    CREATE INDEX IX_ProjectMilestones_ProjectId ON ProjectMilestones(ProjectId) WHERE IsDeleted = 0;
END
GO

PRINT 'Project Management tables created successfully';
GO
