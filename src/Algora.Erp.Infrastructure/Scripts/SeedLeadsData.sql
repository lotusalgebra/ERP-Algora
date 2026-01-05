-- Seed sample leads data
SET QUOTED_IDENTIFIER ON;
GO

INSERT INTO Leads (Id, Name, Company, Email, Phone, Website, Source, Status, Rating, EstimatedValue, EstimatedCloseInDays, Address, City, State, Country, PostalCode, AssignedToId, AssignedToName, LastContactDate, NextFollowUpDate, Notes, Tags, IsDeleted, CreatedAt)
VALUES
(NEWID(), 'John Smith', 'Acme Corporation', 'john.smith@acme.com', '+1-555-0101', 'www.acme.com', 0, 0, 2, 50000.00, 30, '123 Main St', 'New York', 'NY', 'USA', '10001', NULL, NULL, NULL, DATEADD(day, 3, GETUTCDATE()), 'Interested in enterprise solution', 'enterprise,priority', 0, GETUTCDATE()),

(NEWID(), 'Sarah Johnson', 'TechStart Inc', 'sarah@techstart.io', '+1-555-0102', 'www.techstart.io', 2, 1, 1, 15000.00, 45, '456 Oak Ave', 'San Francisco', 'CA', 'USA', '94102', NULL, NULL, DATEADD(day, -2, GETUTCDATE()), DATEADD(day, 5, GETUTCDATE()), 'Follow up on demo request', 'startup,saas', 0, GETUTCDATE()),

(NEWID(), 'Michael Chen', 'Global Trade Ltd', 'mchen@globaltrade.com', '+1-555-0103', 'www.globaltrade.com', 1, 2, 2, 120000.00, 60, '789 Commerce Blvd', 'Chicago', 'IL', 'USA', '60601', NULL, NULL, DATEADD(day, -1, GETUTCDATE()), DATEADD(day, 7, GETUTCDATE()), 'Qualified - needs inventory module', 'manufacturing,hot-lead', 0, GETUTCDATE()),

(NEWID(), 'Emily Davis', 'RetailMax', 'emily.d@retailmax.com', '+1-555-0104', 'www.retailmax.com', 3, 3, 2, 85000.00, 20, '321 Market St', 'Boston', 'MA', 'USA', '02101', NULL, NULL, DATEADD(day, -5, GETUTCDATE()), DATEADD(day, 2, GETUTCDATE()), 'Proposal sent, awaiting response', 'retail,proposal-sent', 0, GETUTCDATE()),

(NEWID(), 'David Wilson', 'BuildRight Construction', 'dwilson@buildright.com', '+1-555-0105', 'www.buildright.com', 4, 1, 1, 35000.00, 90, '555 Builder Way', 'Denver', 'CO', 'USA', '80201', NULL, NULL, DATEADD(day, -3, GETUTCDATE()), DATEADD(day, 10, GETUTCDATE()), 'Needs project management features', 'construction,projects', 0, GETUTCDATE()),

(NEWID(), 'Lisa Anderson', 'HealthPlus Clinic', 'lisa@healthplus.org', '+1-555-0106', 'www.healthplus.org', 0, 0, 0, 25000.00, NULL, '888 Medical Dr', 'Seattle', 'WA', 'USA', '98101', NULL, NULL, NULL, DATEADD(day, 1, GETUTCDATE()), 'New inquiry from website', 'healthcare,new', 0, GETUTCDATE()),

(NEWID(), 'Robert Martinez', 'FastShip Logistics', 'rmartinez@fastship.com', '+1-555-0107', 'www.fastship.com', 1, 4, 2, 200000.00, 14, '999 Shipping Lane', 'Los Angeles', 'CA', 'USA', '90001', NULL, NULL, DATEADD(day, -7, GETUTCDATE()), DATEADD(day, 1, GETUTCDATE()), 'Negotiating contract terms', 'logistics,negotiation', 0, GETUTCDATE()),

(NEWID(), 'Amanda Taylor', 'EduTech Solutions', 'ataylor@edutech.edu', '+1-555-0108', 'www.edutech.edu', 2, 2, 1, 45000.00, 60, '111 Campus Rd', 'Austin', 'TX', 'USA', '78701', NULL, NULL, DATEADD(day, -4, GETUTCDATE()), DATEADD(day, 14, GETUTCDATE()), 'Interested in HR and payroll modules', 'education,hr-payroll', 0, GETUTCDATE()),

(NEWID(), 'James Brown', 'FoodService Pro', 'jbrown@foodservicepro.com', '+1-555-0109', 'www.foodservicepro.com', 5, 5, 0, 18000.00, NULL, '222 Restaurant Row', 'Miami', 'FL', 'USA', '33101', NULL, NULL, DATEADD(day, -30, GETUTCDATE()), NULL, 'Lost to competitor - price sensitivity', 'hospitality,lost', 0, GETUTCDATE()),

(NEWID(), 'Jennifer White', 'GreenEnergy Corp', 'jwhite@greenenergy.com', '+1-555-0110', 'www.greenenergy.com', 3, 3, 2, 150000.00, 30, '333 Solar Ave', 'Phoenix', 'AZ', 'USA', '85001', NULL, NULL, DATEADD(day, -2, GETUTCDATE()), DATEADD(day, 5, GETUTCDATE()), 'Enterprise deal - multiple locations', 'energy,enterprise,multi-site', 0, GETUTCDATE());
GO

PRINT 'Inserted 10 sample leads';
GO
