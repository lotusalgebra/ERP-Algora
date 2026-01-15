-- =============================================
-- Change Default Admin Password
-- IMPORTANT: Run this script IMMEDIATELY after
-- first deployment to secure the admin account
-- =============================================

USE [AlgoraErpAdmin]
GO

-- Generate a new BCrypt hash for your password
-- You can use an online BCrypt generator or run this in C#:
-- BCrypt.Net.BCrypt.HashPassword("YourNewSecurePassword123!")

-- Replace the placeholder hash below with your generated hash
DECLARE @NewPasswordHash NVARCHAR(255) = '$2a$11$REPLACE_WITH_YOUR_BCRYPT_HASH_HERE';
DECLARE @AdminEmail NVARCHAR(255) = 'admin@algora.com';

-- Update the admin password
UPDATE AdminUsers
SET PasswordHash = @NewPasswordHash,
    UpdatedAt = GETUTCDATE()
WHERE Email = @AdminEmail;

IF @@ROWCOUNT > 0
    PRINT 'Successfully updated password for: ' + @AdminEmail;
ELSE
    PRINT 'WARNING: No user found with email: ' + @AdminEmail;

-- Optionally, update the admin email to your company email
-- UPDATE AdminUsers
-- SET Email = 'your-admin@yourcompany.com',
--     UpdatedAt = GETUTCDATE()
-- WHERE Email = 'admin@algora.com';

GO

PRINT '';
PRINT '=============================================';
PRINT 'Password change complete!';
PRINT '';
PRINT 'To generate a BCrypt hash in C#:';
PRINT 'BCrypt.Net.BCrypt.HashPassword("YourPassword")';
PRINT '';
PRINT 'Or use an online generator like:';
PRINT 'https://bcrypt-generator.com/';
PRINT '=============================================';
GO
