-- Drop GstSlab foreign key constraints to allow TaxSlab IDs
-- The GstSlabId column can now store either GstSlabs or TaxSlabs IDs
-- This is needed because TaxSlabs is the new unified tax system

-- Drop FK from InvoiceLines
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_InvoiceLines_GstSlab')
BEGIN
    ALTER TABLE InvoiceLines DROP CONSTRAINT FK_InvoiceLines_GstSlab;
    PRINT 'Dropped FK_InvoiceLines_GstSlab';
END
ELSE
    PRINT 'FK_InvoiceLines_GstSlab does not exist';

-- Drop FK from Invoices (if exists)
IF EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Invoices_GstSlab')
BEGIN
    ALTER TABLE Invoices DROP CONSTRAINT FK_Invoices_GstSlab;
    PRINT 'Dropped FK_Invoices_GstSlab';
END
ELSE
    PRINT 'FK_Invoices_GstSlab does not exist';

-- Rename column comments (for documentation)
-- GstSlabId now represents: TaxSlabId (new system) OR GstSlabId (legacy)

PRINT 'GstSlabId foreign keys removed. Column can now store TaxSlabs or GstSlabs IDs.';
